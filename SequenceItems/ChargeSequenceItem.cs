using Biologic.Native;
using DeviceControlSoftware;
using Serilog;

namespace Biologic.SequenceItems;

/// <summary>
/// Charge battery using potentiostatic (constant voltage) method
/// Uses CA (Chrono-Amperometry) technique to apply voltage steps
/// </summary>
public class ChargeSequenceItem : SequenceItem
{
  private int _deviceId;
  private byte _channelIndex;
  private float _current_A;
  private float _duration_s;
  private float _cutoffVoltage_V;
  private string? _techniquesPath;
  private Dictionary<string, object>? _parameters;

  public override void Initialize(Sequence.StateContext context)
  {
    if (this.Properties == null)
      throw new ArgumentNullException(nameof(this.Properties));

    this._channelIndex = Convert.ToByte(this.Properties.GetValueOrDefault("ChannelIndex") ?? 0);
    this._current_A = Convert.ToSingle(this.Properties.GetValueOrDefault("Current_A") ?? 0.001);
    this._duration_s = Convert.ToSingle(this.Properties.GetValueOrDefault("Duration_s") ?? 3600);
    this._cutoffVoltage_V = Convert.ToSingle(this.Properties.GetValueOrDefault("CutoffVoltage_V") ?? 4.2);

    // Get device ID and techniques path from Device properties
    var device = context.SequenceDispatcher.Devices.Values.FirstOrDefault();
    if (device != null && device.GetProperty("DeviceId") is int deviceId)
    {
      this._deviceId = deviceId;
      this._techniquesPath = device.GetProperty("TechniquesPath") as string;
    }
    else
    {
      throw new InvalidOperationException("Device or DeviceId not found in context");
    }

    // Build CA technique parameters for charging
    // Based on official BioLogic CA technique parameters (see EC-Lab Development Package documentation)
    // All parameters can be overridden from JSON Properties
    this._parameters = new Dictionary<string, object>
    {
      // Step #0: Apply voltage (can be overridden by Voltage_step in JSON)
      ["Voltage_step"] = this.Properties.ContainsKey("Voltage_step")
        ? Convert.ToSingle(this.Properties["Voltage_step"])
        : this._cutoffVoltage_V,

      // vs. initial (can be overridden by vs_initial in JSON)
      ["vs_initial"] = this.Properties.ContainsKey("vs_initial")
        ? Convert.ToBoolean(this.Properties["vs_initial"])
        : false,

      // Step duration (can be overridden by Duration_step in JSON)
      ["Duration_step"] = this.Properties.ContainsKey("Duration_step")
        ? Convert.ToSingle(this.Properties["Duration_step"])
        : this._duration_s,

      // Steps configuration (can be overridden by Step_number in JSON)
      ["Step_number"] = this.Properties.ContainsKey("Step_number")
        ? Convert.ToInt32(this.Properties["Step_number"])
        : 0,

      // Recording parameters (can be overridden in JSON)
      ["Record_every_dI"] = this.Properties.ContainsKey("Record_every_dI")
        ? Convert.ToSingle(this.Properties["Record_every_dI"])
        : 0.01f,

      ["Record_every_dT"] = this.Properties.ContainsKey("Record_every_dT")
        ? Convert.ToSingle(this.Properties["Record_every_dT"])
        : 1.0f,

      // Cycles (can be overridden by N_Cycles in JSON)
      ["N_Cycles"] = this.Properties.ContainsKey("N_Cycles")
        ? Convert.ToInt32(this.Properties["N_Cycles"])
        : 1,

      // Hardware settings (can be overridden in JSON)
      ["I_Range"] = this.Properties.ContainsKey("I_Range")
        ? Convert.ToInt32(this.Properties["I_Range"])
        : (int)I_RANGE.I_RANGE_AUTO,

      ["Bandwidth"] = this.Properties.ContainsKey("Bandwidth")
        ? Convert.ToInt32(this.Properties["Bandwidth"])
        : (int)BANDWIDTH.BW_7
    };
  }

  public override Sequence.ResultTypes Execute(Sequence.StateContext context)
  {
    try
    {
      Log.Information("Starting CA technique on channel {Channel}: Voltage={Voltage}V, Duration={Duration}s",
        this._channelIndex, this._cutoffVoltage_V, this._duration_s);

      // Load CA technique
      if (string.IsNullOrEmpty(this._techniquesPath))
      {
        throw new InvalidOperationException("Techniques path not configured");
      }

      string eccPath = Path.Combine(this._techniquesPath, "ca.ecc");

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
      if (this._parameters != null && this._parameters.Count > 0)
      {
        var paramArray = this.BuildEccParamArray(this._parameters);
        eccParams.len = paramArray.Length;

        // Log parameters for debugging
        Log.Debug("CA Technique Parameters ({Count} total):", paramArray.Length);
        foreach (var p in paramArray)
        {
          string paramName = System.Text.Encoding.ASCII.GetString(p.ParamStr).TrimEnd('\0');
          string valueStr = p.ParamType switch
          {
            (int)PARAM_TYPE.PARAM_BOOLEAN => (p.ParamVal != 0).ToString(),
            (int)PARAM_TYPE.PARAM_INT => p.ParamVal.ToString(),
            (int)PARAM_TYPE.PARAM_SINGLE => BitConverter.ToSingle(BitConverter.GetBytes(p.ParamVal), 0).ToString("F6"),
            _ => p.ParamVal.ToString()
          };
          Log.Debug("  [{Index}] {Name} = {Value} (Type: {Type})",
            p.ParamIndex, paramName, valueStr, (PARAM_TYPE)p.ParamType);
        }

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
          this._deviceId,
          this._channelIndex + 1,
          eccPath,
          ref eccParams,
          first: true,
          last: true,
          display: false);

        // Start channel
        ECLibApi.StartChannel(this._deviceId, this._channelIndex + 1);

        Log.Information("Charge started successfully on channel {Channel}", this._channelIndex);

        context.ResultParameter = new
        {
          Success = true,
          Message = $"Charging started on channel {this._channelIndex}",
          Current_A = this._current_A,
          Duration_s = this._duration_s,
          CutoffVoltage_V = this._cutoffVoltage_V
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

    // CRITICAL: Parameter order MUST EXACTLY match official BioLogic C++ example!
    // From technique_ca/main.cpp (EC-Lab Development Package Examples):
    //
    // Correct order:
    //   [0-2]: Step 0 array params (Voltage_step, vs_initial, Duration_step)
    //   [3-5]: Step 1 array params (if multi-step)
    //   [6-8]: Step 2 array params (if multi-step)
    //   [9]: Step_number
    //   [10]: N_Cycles
    //   [11]: Record_every_dI
    //   [12]: Record_every_dT
    //   [13]: I_Range
    //   [14]: Bandwidth

    // Step 0 indexed array parameters (ParamIndex = step number = 0)
    if (parameters.ContainsKey("Voltage_step"))
    {
      paramList.Add(this.CreateEccParam("Voltage_step", parameters["Voltage_step"], PARAM_TYPE.PARAM_SINGLE, 0));
    }

    if (parameters.ContainsKey("vs_initial"))
    {
      paramList.Add(this.CreateEccParam("vs_initial", parameters["vs_initial"], PARAM_TYPE.PARAM_BOOLEAN, 0));
    }

    if (parameters.ContainsKey("Duration_step"))
    {
      paramList.Add(this.CreateEccParam("Duration_step", parameters["Duration_step"], PARAM_TYPE.PARAM_SINGLE, 0));
    }

    // Scalar parameters (all use ParamIndex = 0)
    if (parameters.ContainsKey("Step_number"))
    {
      paramList.Add(this.CreateEccParam("Step_number", parameters["Step_number"], PARAM_TYPE.PARAM_INT, 0));
    }

    if (parameters.ContainsKey("N_Cycles"))
    {
      paramList.Add(this.CreateEccParam("N_Cycles", parameters["N_Cycles"], PARAM_TYPE.PARAM_INT, 0));
    }

    if (parameters.ContainsKey("Record_every_dI"))
    {
      paramList.Add(this.CreateEccParam("Record_every_dI", parameters["Record_every_dI"], PARAM_TYPE.PARAM_SINGLE, 0));
    }

    if (parameters.ContainsKey("Record_every_dT"))
    {
      paramList.Add(this.CreateEccParam("Record_every_dT", parameters["Record_every_dT"], PARAM_TYPE.PARAM_SINGLE, 0));
    }

    if (parameters.ContainsKey("I_Range"))
    {
      paramList.Add(this.CreateEccParam("I_Range", parameters["I_Range"], PARAM_TYPE.PARAM_INT, 0));
    }

    if (parameters.ContainsKey("Bandwidth"))
    {
      paramList.Add(this.CreateEccParam("Bandwidth", parameters["Bandwidth"], PARAM_TYPE.PARAM_INT, 0));
    }

    return paramList.ToArray();
  }

  private EccParam CreateEccParam(string name, object value, PARAM_TYPE type, int index)
  {
    // Ensure parameter name is not longer than 63 characters (—¯1˜¢Žš??null?Ž~•„)
    if (name.Length > 63)
    {
      throw new ArgumentException($"Parameter name too long: {name} (max 63 characters)");
    }

    var param = new EccParam
    {
      ParamStr = new byte[64], // Must be exactly 64 bytes
      ParamType = (int)type,
      ParamIndex = index
    };

    // Copy parameter name to ParamStr array (null-terminated)
    byte[] nameBytes = System.Text.Encoding.ASCII.GetBytes(name);
    Array.Copy(nameBytes, param.ParamStr, nameBytes.Length);
    // Rest of array is already zero-initialized

    switch (type)
    {
      case PARAM_TYPE.PARAM_BOOLEAN:
        // Store boolean as int (0 or 1) to match C++ int type
        param.ParamVal = Convert.ToBoolean(value) ? 1 : 0;
        break;

      case PARAM_TYPE.PARAM_INT:
        // Store int directly
        param.ParamVal = Convert.ToInt32(value);
        break;

      case PARAM_TYPE.PARAM_SINGLE:
        // Store float as int (reinterpret bytes)
        float floatValue = Convert.ToSingle(value);
        param.ParamVal = BitConverter.ToInt32(BitConverter.GetBytes(floatValue), 0);
        break;
    }

    return param;
  }
}
