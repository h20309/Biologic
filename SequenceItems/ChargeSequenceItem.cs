using Biologic.Devices;
using Biologic.Native;
using DeviceControlSoftware;
using Serilog;

namespace Biologic.SequenceItems;

/// <summary>
/// Charge battery using potentiostatic (constant voltage) method.
/// Uses CA (Chrono-Amperometry) technique to apply voltage steps.
/// </summary>
public class ChargeSequenceItem : SequenceItem
{
  private const string PreferredCaVsParameterName = "vs_initial";
  private const string FallbackCaVsParameterName = "vs_initializer";
  private const float DefaultRecordEveryDt = 1.0f;
  private const float DefaultRecordEveryDi = 0.01f;

  private int _deviceId;
  private byte _channelIndex;
  private float _current_A;
  private float _duration_s;
  private float _cutoffVoltage_V;
  private float _recordInterval_s;
  private Dictionary<string, object>? _parameters;
  private ECLabDevice? _device;
  private bool _useVsInitializerLabel;
  private int _defaultIRange = (int)I_RANGE.I_RANGE_AUTO;
  private int _defaultBandwidth = (int)BANDWIDTH.BW_7;

  public override void Initialize(Sequence.StateContext context)
  {
    if (this.Properties == null)
      throw new ArgumentNullException(nameof(this.Properties));

    if (context.MethodParameter is Biologic.MethodParameters.Charge methodParameter)
    {
      this._channelIndex = Convert.ToByte(methodParameter.ChannelIndex);
      this._current_A = Convert.ToSingle(this.Properties.GetValueOrDefault("Current_A") ?? 0.0f);
      this._duration_s = methodParameter.Duration_s;
      this._cutoffVoltage_V = methodParameter.Voltage_V;
      this._recordInterval_s = methodParameter.RecordInterval_s;
    }
    else
    {
      this._channelIndex = Convert.ToByte(this.Properties.GetValueOrDefault("ChannelIndex") ?? 0);
      this._current_A = Convert.ToSingle(this.Properties.GetValueOrDefault("Current_A") ?? 0.001f);
      this._duration_s = Convert.ToSingle(this.Properties.GetValueOrDefault("Duration_s") ?? 3600.0f);
      this._cutoffVoltage_V = Convert.ToSingle(
        this.Properties.GetValueOrDefault("Voltage_V") ??
        this.Properties.GetValueOrDefault("CutoffVoltage_V") ??
        4.2f);
      this._recordInterval_s = Convert.ToSingle(
        this.Properties.GetValueOrDefault("RecordInterval_s") ??
        this.Properties.GetValueOrDefault("Record EVERY_dT") ??
        this.Properties.GetValueOrDefault("Record_every_dT") ??
        DefaultRecordEveryDt);
    }

    var device = context.SequenceDispatcher.Devices.Values.FirstOrDefault();
    if (device is ECLabDevice ecLabDevice && device.GetProperty("DeviceId") is int deviceId)
    {
      this._device = ecLabDevice;
      this._deviceId = deviceId;

      ChannelInfo channelInfo = ECLibApi.GetChannelInfo(this._deviceId, this._channelIndex + 1);
      if (channelInfo.MaxIRange >= 0)
      {
        this._defaultIRange = channelInfo.MaxIRange;
      }

      if (channelInfo.MaxBandwidth > 0)
      {
        this._defaultBandwidth = channelInfo.MaxBandwidth;
      }
    }
    else
    {
      throw new InvalidOperationException("Device or DeviceId not found in context");
    }

    this._useVsInitializerLabel = this.Properties.ContainsKey(FallbackCaVsParameterName) &&
      !this.Properties.ContainsKey(PreferredCaVsParameterName);
    this._parameters = this.BuildChargeParameters(this._useVsInitializerLabel);
  }

  public override Sequence.ResultTypes Execute(Sequence.StateContext context)
  {
    try
    {
      if (this._device == null || !this._device.IsConnected || this._device.DeviceId < 0)
      {
        throw new InvalidOperationException("Device is not connected");
      }

      Log.Information("Starting CA technique on channel {Channel}: Voltage={Voltage}V, Duration={Duration}s",
        this._channelIndex, this._cutoffVoltage_V, this._duration_s);

      var currentValues = ECLibApi.GetCurrentValues(this._deviceId, this._channelIndex + 1);
      Log.Information(
        "Channel {Channel} live ranges before CA: Ewe={Ewe}V, ERange=[{Min}V, {Max}V], I={Current}A, IRange={IRange}",
        this._channelIndex,
        currentValues.Ewe,
        currentValues.EweRangeMin,
        currentValues.EweRangeMax,
        currentValues.I,
        currentValues.IRange);

      ValidateRequestedVoltageRange(this._cutoffVoltage_V, currentValues.EweRangeMin, currentValues.EweRangeMax);

      this.LoadChargeTechniqueWithFallbacks("ca.ecc");
      ECLibApi.StartChannel(this._deviceId, this._channelIndex + 1);

      Log.Information("Charge started successfully on channel {Channel}", this._channelIndex);

      context.ResultParameter = new
      {
        Success = true,
        Message = $"Charging started on channel {this._channelIndex}",
        Voltage_V = this._cutoffVoltage_V,
        Duration_s = this._duration_s,
        RecordInterval_s = this._recordInterval_s
      };

      return Sequence.ResultTypes.Next;
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
  }

  private Dictionary<string, object> BuildChargeParameters(bool useVsInitializerLabel, bool includeHardwareParameters = false, int? iRange = null, int? bandwidth = null, int? eRange = null)
  {
    return this.BuildChargeParameters(
      useVsInitializerLabel,
      includeHardwareParameters,
      iRange,
      bandwidth,
      eRange,
      "Record EVERY_dI",
      "Record EVERY_dT",
      1);
  }

  private Dictionary<string, object> BuildChargeParameters(
    bool useVsInitializerLabel,
    bool includeHardwareParameters,
    int? iRange,
    int? bandwidth,
    int? eRange,
    string recordDiLabel,
    string recordDtLabel,
    int stepCount)
  {
    string vsParameterName = useVsInitializerLabel ? FallbackCaVsParameterName : PreferredCaVsParameterName;
    bool vsInitialValue = this.Properties!.ContainsKey(FallbackCaVsParameterName)
      ? Convert.ToBoolean(this.Properties[FallbackCaVsParameterName])
      : this.Properties.ContainsKey(PreferredCaVsParameterName)
        ? Convert.ToBoolean(this.Properties[PreferredCaVsParameterName])
        : false;

    float voltageStep = this.Properties.ContainsKey("Voltage_step")
      ? Convert.ToSingle(this.Properties["Voltage_step"])
      : this._cutoffVoltage_V;
    float durationStep = this.Properties.ContainsKey("Duration_step")
      ? Convert.ToSingle(this.Properties["Duration_step"])
      : this._duration_s;

    object voltageValue = stepCount > 1 ? CreateRepeatedFloatArray(voltageStep, stepCount) : voltageStep;
    object durationValue = stepCount > 1 ? CreateRepeatedFloatArray(durationStep, stepCount) : durationStep;
    object vsValue = stepCount > 1 ? CreateRepeatedBoolArray(vsInitialValue, stepCount) : vsInitialValue;

    var parameters = new Dictionary<string, object>
    {
      ["Voltage_step"] = voltageValue,
      [vsParameterName] = vsValue,
      ["Duration_step"] = durationValue,
      ["Step_number"] = this.Properties.ContainsKey("Step_number")
        ? Convert.ToInt32(this.Properties["Step_number"])
        : stepCount - 1,
      ["N_Cycles"] = this.Properties.ContainsKey("N_Cycles")
        ? Convert.ToInt32(this.Properties["N_Cycles"])
        : 1,
      [recordDiLabel] = this.Properties.ContainsKey(recordDiLabel)
        ? Convert.ToSingle(this.Properties[recordDiLabel])
        : recordDiLabel == "Record EVERY_dI" && this.Properties.ContainsKey("Record_every_dI")
          ? Convert.ToSingle(this.Properties["Record_every_dI"])
        : this.Properties.ContainsKey("Record_every_dI")
          ? Convert.ToSingle(this.Properties["Record_every_dI"])
          : DefaultRecordEveryDi,
      [recordDtLabel] = this.Properties.ContainsKey(recordDtLabel)
        ? Convert.ToSingle(this.Properties[recordDtLabel])
        : recordDtLabel == "Record EVERY_dT" && this.Properties.ContainsKey("Record_every_dT")
          ? Convert.ToSingle(this.Properties["Record_every_dT"])
        : this.Properties.ContainsKey("Record_every_dT")
          ? Convert.ToSingle(this.Properties["Record_every_dT"])
          : this._recordInterval_s
    };

    if (includeHardwareParameters)
    {
      parameters["I_Range"] = this.Properties.ContainsKey("I_Range")
        ? Convert.ToInt32(this.Properties["I_Range"])
        : iRange ?? this._defaultIRange;
      parameters["E_Range"] = this.Properties.ContainsKey("E_Range")
        ? Convert.ToInt32(this.Properties["E_Range"])
        : eRange ?? SelectERange(this._cutoffVoltage_V);
      parameters["Bandwidth"] = this.Properties.ContainsKey("Bandwidth")
        ? Convert.ToInt32(this.Properties["Bandwidth"])
        : bandwidth ?? this._defaultBandwidth;
    }

    return parameters;
  }

  private void LoadChargeTechniqueWithFallbacks(string preferredTechniqueFile)
  {
    string[] techniqueCandidates = [preferredTechniqueFile, "ca.ecc", "ca4.ecc", "ca5.ecc"];
    var attempts = new List<(string Name, Dictionary<string, object> Parameters)>();
    int resolvedERange = SelectERange(this._cutoffVoltage_V);
    attempts.Add(("vs_initializer/manual-minimal", this.BuildChargeParameters(true, false, null, null, null, "Record EVERY_dI", "Record EVERY_dT", 1)));
    attempts.Add(("vs_initializer/manual-three-step", this.BuildChargeParameters(true, false, null, null, null, "Record EVERY_dI", "Record EVERY_dT", 3)));
    attempts.Add(("vs_initial/example-minimal", this.BuildChargeParameters(false, false, null, null, null, "Record_every_dI", "Record_every_dT", 1)));
    attempts.Add(("vs_initial/example-three-step", this.BuildChargeParameters(false, false, null, null, null, "Record_every_dI", "Record_every_dT", 3)));
    attempts.Add(("vs_initial/example-three-step-uppercase-records", this.BuildChargeParameters(false, false, null, null, null, "Record EVERY_dI", "Record EVERY_dT", 3)));
    attempts.Add(("vs_initializer/with-e-range", this.BuildChargeParameters(true, true, null, null, resolvedERange, "Record EVERY_dI", "Record EVERY_dT", 1)));
    attempts.Add(("vs_initial/with-e-range", this.BuildChargeParameters(false, true, null, null, resolvedERange, "Record_every_dI", "Record_every_dT", 1)));
    attempts.Add(("vs_initializer/with-auto-e-range", this.BuildChargeParameters(true, true, null, null, (int)E_RANGE.E_RANGE_AUTO, "Record EVERY_dI", "Record EVERY_dT", 1)));
    attempts.Add(("vs_initial/with-auto-e-range", this.BuildChargeParameters(false, true, null, null, (int)E_RANGE.E_RANGE_AUTO, "Record_every_dI", "Record_every_dT", 1)));
    attempts.Add(("vs_initial/with-hardware", this.BuildChargeParameters(false, true, null, null, null, "Record_every_dI", "Record_every_dT", 1)));
    attempts.Add(("vs_initializer/with-hardware", this.BuildChargeParameters(true, true, null, null, null, "Record EVERY_dI", "Record EVERY_dT", 1)));
    attempts.Add(("vs_initial/with-auto-range", this.BuildChargeParameters(false, true, (int)I_RANGE.I_RANGE_AUTO, (int)BANDWIDTH.BW_7, (int)E_RANGE.E_RANGE_AUTO, "Record_every_dI", "Record_every_dT", 1)));
    attempts.Add(("vs_initializer/with-auto-range", this.BuildChargeParameters(true, true, (int)I_RANGE.I_RANGE_AUTO, (int)BANDWIDTH.BW_7, (int)E_RANGE.E_RANGE_AUTO, "Record EVERY_dI", "Record EVERY_dT", 1)));

    Exception? lastException = null;

    foreach (string techniqueFile in techniqueCandidates.Distinct(StringComparer.OrdinalIgnoreCase))
    {
      string eccPath = this._device!.ResolveTechniqueFilePath(techniqueFile, this._channelIndex);
      if (!File.Exists(eccPath))
      {
        continue;
      }

      string nativeEccPath = this._device.GetNativeTechniqueFilePath(techniqueFile, this._channelIndex);
      Log.Information(
        "Trying CA technique file: {TechniquePath} (native path: {NativeTechniquePath})",
        eccPath,
        nativeEccPath);

      foreach (var attempt in attempts)
      {
        var eccParams = new EccParams
        {
          len = 0,
          pParams = IntPtr.Zero
        };

        try
        {
          this._parameters = attempt.Parameters;
          Log.Information("Trying CA parameter variant: {VariantName}", $"{techniqueFile}/{attempt.Name}");
          this.PopulateEccParams(ref eccParams, this.BuildEccParamArray(attempt.Parameters));

          ECLibApi.LoadTechnique(
            this._deviceId,
            this._channelIndex + 1,
            nativeEccPath,
            ref eccParams,
            first: true,
            last: true,
            display: false);

          this._useVsInitializerLabel = attempt.Name.Contains("vs_initializer", StringComparison.Ordinal);
          return;
        }
        catch (ECLibException ex) when (ex.ErrorCode == (int)BL_ERROR.GEN_INVALIDPARAMETERS || ex.ErrorCode == (int)BL_ERROR.TECH_LOADTECHNIQUEFAILED)
        {
          lastException = ex;
          Log.Warning("CA parameter variant {VariantName} failed: {Message}", $"{techniqueFile}/{attempt.Name}", ex.Message);
          this.LogChannelDiagnostics("CA", $"{techniqueFile}/{attempt.Name}");
        }
        finally
        {
          if (eccParams.pParams != IntPtr.Zero)
          {
            System.Runtime.InteropServices.Marshal.FreeHGlobal(eccParams.pParams);
          }
        }
      }
    }

    throw lastException ?? new InvalidOperationException("No CA parameter variant was attempted.");
  }

  private void PopulateEccParams(ref EccParams eccParams, EccParam[] paramArray)
  {
    eccParams.len = paramArray.Length;

    Log.Information("CA Technique Parameters ({Count} total):", paramArray.Length);
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
      Log.Information("  [{Index}] {Name} = {Value} (Type: {Type})",
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

  private EccParam[] BuildEccParamArray(Dictionary<string, object> parameters)
  {
    var paramList = new List<EccParam>();

    if (parameters.ContainsKey("Voltage_step"))
    {
      this.AddSingleParameters(paramList, "Voltage_step", parameters["Voltage_step"]);
    }

    if (parameters.ContainsKey("vs_initializer"))
    {
      this.AddBoolParameters(paramList, "vs_initializer", parameters["vs_initializer"]);
    }

    if (parameters.ContainsKey("vs_initial"))
    {
      this.AddBoolParameters(paramList, "vs_initial", parameters["vs_initial"]);
    }

    if (parameters.ContainsKey("Duration_step"))
    {
      this.AddSingleParameters(paramList, "Duration_step", parameters["Duration_step"]);
    }

    if (parameters.ContainsKey("Step_number"))
    {
      paramList.Add(ECLibApi.DefineIntParameter("Step_number", Convert.ToInt32(parameters["Step_number"]), 0));
    }

    if (parameters.ContainsKey("N_Cycles"))
    {
      paramList.Add(ECLibApi.DefineIntParameter("N_Cycles", Convert.ToInt32(parameters["N_Cycles"]), 0));
    }

    if (parameters.ContainsKey("Record_every_dI"))
    {
      paramList.Add(ECLibApi.DefineSingleParameter("Record_every_dI", Convert.ToSingle(parameters["Record_every_dI"]), 0));
    }

    if (parameters.ContainsKey("Record EVERY_dI"))
    {
      paramList.Add(ECLibApi.DefineSingleParameter("Record EVERY_dI", Convert.ToSingle(parameters["Record EVERY_dI"]), 0));
    }

    if (parameters.ContainsKey("Record_every_dT"))
    {
      paramList.Add(ECLibApi.DefineSingleParameter("Record_every_dT", Convert.ToSingle(parameters["Record_every_dT"]), 0));
    }

    if (parameters.ContainsKey("Record EVERY_dT"))
    {
      paramList.Add(ECLibApi.DefineSingleParameter("Record EVERY_dT", Convert.ToSingle(parameters["Record EVERY_dT"]), 0));
    }

    if (parameters.ContainsKey("I_Range"))
    {
      paramList.Add(ECLibApi.DefineIntParameter("I_Range", Convert.ToInt32(parameters["I_Range"]), 0));
    }

    if (parameters.ContainsKey("E_Range"))
    {
      paramList.Add(ECLibApi.DefineIntParameter("E_Range", Convert.ToInt32(parameters["E_Range"]), 0));
    }

    if (parameters.ContainsKey("Bandwidth"))
    {
      paramList.Add(ECLibApi.DefineIntParameter("Bandwidth", Convert.ToInt32(parameters["Bandwidth"]), 0));
    }

    return paramList.ToArray();
  }

  private void AddSingleParameters(List<EccParam> paramList, string label, object value)
  {
    if (value is Array array)
    {
      for (int i = 0; i < array.Length; i++)
      {
        paramList.Add(ECLibApi.DefineSingleParameter(label, Convert.ToSingle(array.GetValue(i)), i));
      }

      return;
    }

    paramList.Add(ECLibApi.DefineSingleParameter(label, Convert.ToSingle(value), 0));
  }

  private void AddBoolParameters(List<EccParam> paramList, string label, object value)
  {
    if (value is Array array)
    {
      for (int i = 0; i < array.Length; i++)
      {
        paramList.Add(ECLibApi.DefineBoolParameter(label, Convert.ToBoolean(array.GetValue(i)), i));
      }

      return;
    }

    paramList.Add(ECLibApi.DefineBoolParameter(label, Convert.ToBoolean(value), 0));
  }

  private void LogChannelDiagnostics(string techniqueName, string variantName)
  {
    foreach (string message in ECLibApi.DrainChannelMessages(this._deviceId, this._channelIndex + 1))
    {
      Log.Warning("{TechniqueName} channel message after {VariantName}: {Message}", techniqueName, variantName, message);
    }

    var optError = ECLibApi.TryGetOptionError(this._deviceId, this._channelIndex + 1);
    if (optError is { OptError: not 0 } details)
    {
      Log.Warning(
        "{TechniqueName} option error after {VariantName}: Code={OptError}, Position={OptPos}",
        techniqueName,
        variantName,
        details.OptError,
        details.OptPos);
    }
  }

  private static float[] CreateRepeatedFloatArray(float value, int count)
  {
    var values = new float[count];
    for (int i = 0; i < count; i++)
    {
      values[i] = value;
    }

    return values;
  }

  private static bool[] CreateRepeatedBoolArray(bool value, int count)
  {
    var values = new bool[count];
    for (int i = 0; i < count; i++)
    {
      values[i] = value;
    }

    return values;
  }

  private static int SelectERange(float voltage)
  {
    float absoluteVoltage = Math.Abs(voltage);
    if (absoluteVoltage <= 2.5f)
    {
      return (int)E_RANGE.E_RANGE_2_5V;
    }

    if (absoluteVoltage <= 5.0f)
    {
      return (int)E_RANGE.E_RANGE_5V;
    }

    if (absoluteVoltage <= 10.0f)
    {
      return (int)E_RANGE.E_RANGE_10V;
    }

    return (int)E_RANGE.E_RANGE_AUTO;
  }

  private static void ValidateRequestedVoltageRange(float requestedVoltage, float liveRangeMin, float liveRangeMax)
  {
    if (requestedVoltage < liveRangeMin || requestedVoltage > liveRangeMax)
    {
      throw new InvalidOperationException(
        $"Requested charge voltage {requestedVoltage:F3} V is outside the current channel range [{liveRangeMin:F3} V, {liveRangeMax:F3} V]. " +
        "On the current VMP3-family channel, this is a hardware range limit rather than a technique-loading problem. Use a lower target voltage that fits the live range, or change the external hardware/wiring setup to one that supports a wider voltage range.");
    }
  }
}
