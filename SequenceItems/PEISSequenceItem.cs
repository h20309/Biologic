using Biologic.Devices;
using Biologic.Native;
using DeviceControlSoftware;
using Serilog;

namespace Biologic.SequenceItems;

/// <summary>
/// Potentiostatic Electrochemical Impedance Spectroscopy (PEIS)
/// Applies AC voltage and measures impedance at different frequencies
/// </summary>
public class PEISSequenceItem : SequenceItem
{
  private int _deviceId;
  private byte _channelIndex;
  private float _initialFrequency_Hz;
  private float _finalFrequency_Hz;
  private int _frequencyPoints;
  private float _dcVoltage_V;
  private float _acAmplitude_V;
  private Dictionary<string, object>? _parameters;

  public override void Initialize(Sequence.StateContext context)
  {
    if (Properties == null)
      throw new ArgumentNullException(nameof(Properties));

    _channelIndex = Convert.ToByte(Properties.GetValueOrDefault("ChannelIndex") ?? 0);
    _initialFrequency_Hz = Convert.ToSingle(Properties.GetValueOrDefault("InitialFrequency_Hz") ?? 100000);
    _finalFrequency_Hz = Convert.ToSingle(Properties.GetValueOrDefault("FinalFrequency_Hz") ?? 0.01);
    _frequencyPoints = Convert.ToInt32(Properties.GetValueOrDefault("FrequencyPoints") ?? 10);
    _dcVoltage_V = Convert.ToSingle(Properties.GetValueOrDefault("DcVoltage_V") ?? 0.0);
    _acAmplitude_V = Convert.ToSingle(Properties.GetValueOrDefault("AcAmplitude_V") ?? 0.01);

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

    // Build PEIS technique parameters
    _parameters = new Dictionary<string, object>
    {
      ["Initial_Frequency"] = _initialFrequency_Hz,
      ["Final_Frequency"] = _finalFrequency_Hz,
      ["Number_of_frequencies"] = _frequencyPoints,
      ["Dc_Voltage"] = _dcVoltage_V,
      ["Amplitude_Voltage"] = _acAmplitude_V,
      ["Frequency_spacing"] = 1, // Logarithmic
      ["Wait_for_steady"] = 0.1f,
      ["Record_every_dE"] = 0.001f
    };
  }

  public override Sequence.ResultTypes Execute(Sequence.StateContext context)
  {
    try
    {
      Log.Information("Starting PEIS on channel {Channel}: {InitFreq}Hz to {FinalFreq}Hz, {Points} points",
        _channelIndex, _initialFrequency_Hz, _finalFrequency_Hz, _frequencyPoints);

      // Load PEIS technique
      string eccPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "Techniques",
        "peis.ecc");

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

        Log.Information("PEIS started successfully on channel {Channel}", _channelIndex);

        context.ResultParameter = new
        {
          Success = true,
          Message = $"PEIS started on channel {_channelIndex}",
          InitialFrequency_Hz = _initialFrequency_Hz,
          FinalFrequency_Hz = _finalFrequency_Hz,
          FrequencyPoints = _frequencyPoints,
          DcVoltage_V = _dcVoltage_V,
          AcAmplitude_V = _acAmplitude_V
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
      Log.Error("PEIS failed: {ErrorMessage} (Code: {ErrorCode})", ex.Message, ex.ErrorCode);

      context.ResultParameter = new
      {
        Success = false,
        Message = $"Failed to start PEIS: {ex.Message}"
      };

      return Sequence.ResultTypes.Error;
    }
    catch (Exception ex)
    {
      Log.Error(ex, "Unexpected error in PEIS");

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
