using Biologic.Devices;
using Biologic.Native;
using DeviceControlSoftware;
using Serilog;

namespace Biologic.SequenceItems;

/// <summary>
/// Charge battery using galvanostatic (constant current) method
/// Uses CA (Chrono-Amperometry) technique with positive current
/// </summary>
public class ChargeSequenceItem : SequenceItem
{
  private int _deviceId;
  private byte _channelIndex;
  private float _current_A;
  private float _duration_s;
  private float _cutoffVoltage_V;
  private Dictionary<string, object>? _parameters;

  public override void Initialize(Sequence.StateContext context)
  {
    if (Properties == null)
      throw new ArgumentNullException(nameof(Properties));

    _channelIndex = Convert.ToByte(Properties.GetValueOrDefault("ChannelIndex") ?? 0);
    _current_A = Convert.ToSingle(Properties.GetValueOrDefault("Current_A") ?? 0.001);
    _duration_s = Convert.ToSingle(Properties.GetValueOrDefault("Duration_s") ?? 3600);
    _cutoffVoltage_V = Convert.ToSingle(Properties.GetValueOrDefault("CutoffVoltage_V") ?? 4.2);

    // Get device ID from Device properties
    var device = context.SequenceDispatcher.Devices.Values.FirstOrDefault();
    if (device != null && device.GetProperty("DeviceId") is int deviceId)
    {
      _deviceId = deviceId;
    }
    else
    {
      throw new InvalidOperationException("Device or DeviceId not found in context");
    }

    // Build CA technique parameters for charging
    _parameters = new Dictionary<string, object>
    {
      ["Current_step"] = Math.Abs(_current_A), // Ensure positive for charging
      ["Duration_step"] = _duration_s,
      ["Voltage_limit"] = _cutoffVoltage_V,
      ["Record_every_dE"] = 0.01f,
      ["Record_every_dt"] = 1.0f,
      ["N_Cycles"] = 1
    };
  }

  public override Sequence.ResultTypes Execute(Sequence.StateContext context)
  {
    try
    {
      Log.Information("Starting charge on channel {Channel}: Current={Current}A, Duration={Duration}s, Cutoff={Cutoff}V",
        _channelIndex, _current_A, _duration_s, _cutoffVoltage_V);

      // Load CA technique
      string eccPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "Techniques",
        "ca.ecc");

      if (!File.Exists(eccPath))
      {
        throw new FileNotFoundException($"Technique file not found: {eccPath}");
      }

      // Create EccParams structure
      var eccParams = new EccParams
      {
        len = 0,
        pParams = IntPtr.Zero
      };

      // Build parameter array
      if (_parameters != null && _parameters.Count > 0)
      {
        var paramArray = BuildEccParamArray(_parameters);
        eccParams.len = paramArray.Length;

        int paramSize = System.Runtime.InteropServices.Marshal.SizeOf<EccParam>();
        eccParams.pParams = System.Runtime.InteropServices.Marshal.AllocHGlobal(paramSize * paramArray.Length);

        for (int i = 0; i < paramArray.Length; i++)
        {
          IntPtr ptr = IntPtr.Add(eccParams.pParams, i * paramSize);
          System.Runtime.InteropServices.Marshal.StructureToPtr(paramArray[i], ptr, false);
        }
      }

      try
      {
        // Load technique
        ECLibApi.LoadTechnique(
          _deviceId,
          _channelIndex + 1,
          eccPath,
          ref eccParams,
          first: true,
          last: true,
          display: false);

        // Start channel
        ECLibApi.StartChannel(_deviceId, _channelIndex + 1);

        Log.Information("Charge started successfully on channel {Channel}", _channelIndex);

        context.ResultParameter = new
        {
          Success = true,
          Message = $"Charging started on channel {_channelIndex}",
          Current_A = _current_A,
          Duration_s = _duration_s,
          CutoffVoltage_V = _cutoffVoltage_V
        };

        return Sequence.ResultTypes.Next;
      }
      finally
      {
        if (eccParams.pParams != IntPtr.Zero)
        {
          System.Runtime.InteropServices.Marshal.FreeHGlobal(eccParams.pParams);
        }
      }
    }
    catch (ECLibException ex)
    {
      Log.Error("Charge failed: {ErrorMessage} (Code: {ErrorCode})", ex.Message, ex.ErrorCode);

      context.ResultParameter = new
      {
        Success = false,
        Message = $"Failed to start charge: {ex.Message}"
      };

      return Sequence.ResultTypes.Error;
    }
    catch (Exception ex)
    {
      Log.Error(ex, "Unexpected error in Charge");

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

      var value = kvp.Value;
      if (value is bool boolValue)
      {
        param.ParamType = (int)PARAM_TYPE.PARAM_BOOLEAN;
        param.ParamVal = boolValue ? 1u : 0u;
      }
      else if (value is int intValue)
      {
        param.ParamType = (int)PARAM_TYPE.PARAM_INT;
        param.ParamVal = (uint)intValue;
      }
      else if (value is float floatValue)
      {
        param.ParamType = (int)PARAM_TYPE.PARAM_SINGLE;
        param.ParamVal = BitConverter.ToUInt32(BitConverter.GetBytes(floatValue), 0);
      }
      else if (value is double doubleValue)
      {
        param.ParamType = (int)PARAM_TYPE.PARAM_SINGLE;
        float f = (float)doubleValue;
        param.ParamVal = BitConverter.ToUInt32(BitConverter.GetBytes(f), 0);
      }

      paramList.Add(param);
    }

    return paramList.ToArray();
  }
}
