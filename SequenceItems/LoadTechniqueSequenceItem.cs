using Biologic.Native;
using Biologic.Devices;
using System.Text.Json;
using DeviceControlSoftware;
using Serilog;

namespace Biologic.SequenceItems;

/// <summary>
/// Load technique from .ecc file
/// Uses ECLibApi for type-safe technique loading
/// </summary>
public class LoadTechniqueSequenceItem : SequenceItem
{
  private int _deviceId;
  private byte _channelIndex;
  private string _techniqueFile = string.Empty;
  private Dictionary<string, object>? _parameters;
  private ECLabDevice? _device;

  public override void Initialize(Sequence.StateContext context)
  {
    if (Properties == null)
      throw new ArgumentNullException(nameof(Properties));

    // Parse properties
    _techniqueFile = Properties.GetValueOrDefault("TechniqueFile")?.ToString() ?? "ocv.ecc";
    _channelIndex = Convert.ToByte(Properties.GetValueOrDefault("ChannelIndex") ?? 0);

    if (Properties.ContainsKey("Parameters"))
    {
      _parameters = Properties["Parameters"] as Dictionary<string, object>;
    }

    // Get device ID from Device properties
    var device = context.SequenceDispatcher.Devices.Values.FirstOrDefault();
    if (device is ECLabDevice ecLabDevice && device.GetProperty("DeviceId") is int deviceId)
    {
      _device = ecLabDevice;
      _deviceId = deviceId;
    }
    else
    {
      throw new InvalidOperationException("Device or DeviceId not found in context");
    }
  }

  public override Sequence.ResultTypes Execute(Sequence.StateContext context)
  {
    try
    {
      if (_device == null)
      {
        throw new InvalidOperationException("ECLab device not available for technique loading");
      }

      if (!_device.IsConnected || _device.DeviceId < 0)
      {
        throw new InvalidOperationException("Device is not connected");
      }

      string eccPath = _device.ResolveTechniqueFilePath(_techniqueFile, _channelIndex);
      string nativeEccPath = _device.GetNativeTechniqueFilePath(_techniqueFile, _channelIndex);

      if (!File.Exists(eccPath))
      {
        throw new FileNotFoundException($"Technique file not found: {eccPath}");
      }

      Log.Information(
        "Loading technique from {TechniqueFile} to channel {Channel} (native path: {NativeTechniqueFile})",
        eccPath,
        _channelIndex,
        nativeEccPath);

      // Create EccParams structure
      var eccParams = new EccParams
      {
        len = 0,
        pParams = IntPtr.Zero
      };

      // If parameters provided, build EccParam array
      if (_parameters != null && _parameters.Count > 0)
      {
        var paramArray = BuildEccParamArray(_parameters);
        eccParams.len = paramArray.Length;

        // Allocate unmanaged memory for parameters
        int paramSize = System.Runtime.InteropServices.Marshal.SizeOf<EccParam>();
        eccParams.pParams = System.Runtime.InteropServices.Marshal.AllocHGlobal(paramSize * paramArray.Length);

        // Copy parameters to unmanaged memory
        for (int i = 0; i < paramArray.Length; i++)
        {
          IntPtr ptr = IntPtr.Add(eccParams.pParams, i * paramSize);
          System.Runtime.InteropServices.Marshal.StructureToPtr(paramArray[i], ptr, false);
        }
      }

      try
      {
        // Load technique using ECLibApi (converts to 1-based internally)
        ECLibApi.LoadTechnique(
            _deviceId,
            _channelIndex + 1, // Convert to 1-based
          nativeEccPath,
            ref eccParams,
            first: true,
            last: true,
            display: false);

        Log.Information("Technique loaded successfully on channel {Channel}", _channelIndex);

        context.ResultParameter = new
        {
          Success = true,
          Message = $"Technique loaded: {_techniqueFile}"
        };

        return Sequence.ResultTypes.Next;
      }
      finally
      {
        // Free unmanaged memory
        if (eccParams.pParams != IntPtr.Zero)
        {
          System.Runtime.InteropServices.Marshal.FreeHGlobal(eccParams.pParams);
        }
      }
    }
    catch (ECLibException ex)
    {
      Log.Error("LoadTechnique failed: {ErrorMessage} (Code: {ErrorCode})", ex.Message, ex.ErrorCode);

      context.ResultParameter = new
      {
        Success = false,
        Message = $"Failed to load technique: {ex.Message}"
      };

      return Sequence.ResultTypes.Error;
    }
    catch (Exception ex)
    {
      Log.Error(ex, "Unexpected error in LoadTechnique");

      context.ResultParameter = new
      {
        Success = false,
        Message = $"Unexpected error: {ex.Message}"
      };

      return Sequence.ResultTypes.Error;
    }
  }

  public override void Deinitialize(Sequence.StateContext context)
  {
    // No cleanup needed
  }

  private EccParam[] BuildEccParamArray(Dictionary<string, object> parameters)
  {
    var paramList = new List<EccParam>();
    int index = 0;

    foreach (var kvp in parameters)
    {
      var param = new EccParam
      {
        ParamStr = System.Text.Encoding.ASCII.GetBytes(kvp.Key.PadRight(64, '\0')),
        ParamIndex = index++
      };

      // Determine parameter type and value
      var value = kvp.Value;
      if (value is bool boolValue)
      {
        param.ParamType = (int)PARAM_TYPE.PARAM_BOOLEAN;
        param.ParamVal = boolValue ? 1 : 0;
      }
      else if (value is int intValue)
      {
        param.ParamType = (int)PARAM_TYPE.PARAM_INT;
        param.ParamVal = intValue;
      }
      else if (value is float floatValue)
      {
        param.ParamType = (int)PARAM_TYPE.PARAM_SINGLE;
        param.ParamVal = BitConverter.ToInt32(BitConverter.GetBytes(floatValue), 0);
      }
      else if (value is double doubleValue)
      {
        param.ParamType = (int)PARAM_TYPE.PARAM_SINGLE;
        float f = (float)doubleValue;
        param.ParamVal = BitConverter.ToInt32(BitConverter.GetBytes(f), 0);
      }

      paramList.Add(param);
    }

    return paramList.ToArray();
  }
}
