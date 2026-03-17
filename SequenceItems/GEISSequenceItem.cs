using Biologic.Devices;
using Biologic.Native;
using DeviceControlSoftware;
using Serilog;
using System.Text;

namespace Biologic.SequenceItems;

/// <summary>
/// Galvanostatic Electrochemical Impedance Spectroscopy (GEIS)
/// Applies AC current and measures impedance at different frequencies
/// </summary>
public class GEISSequenceItem : SequenceItem
{
  private const float DefaultRecordEveryDt = 0.0f;
  private const float DefaultRecordEveryDe = 0.0f;
  private const float DefaultWaitForSteady = 0.0f;
  private const float DefaultDurationStep = 1.0f;
  private const float DefaultNonZeroCurrentStep = 1e-6f;
  private const float DefaultConservativeAmplitude = 10e-6f;
  private const float DefaultConservativeInitialFrequency = 10000.0f;
  private const float DefaultConservativeFinalFrequency = 1.0f;
  private const int DefaultConservativeFrequencyPoints = 10;
  private const float DefaultConservativeWaitForSteady = 1.0f;
  private const int DefaultAverageCount = 1;
  private static readonly TimeSpan DefaultPollInterval = TimeSpan.FromMilliseconds(200);
  private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(5);
  private static readonly TimeSpan StopWaitTimeout = TimeSpan.FromSeconds(5);

  private int _deviceId;
  private byte _channelIndex;
  private float _initialFrequency_Hz;
  private float _finalFrequency_Hz;
  private int _frequencyPoints;
  private float _dcCurrent_A;
  private float _acAmplitude_A;
  private string? _outputFile;
  private Dictionary<string, object>? _parameters;
  private ECLabDevice? _device;

  public override void Initialize(Sequence.StateContext context)
  {
    if (Properties == null)
      throw new ArgumentNullException(nameof(Properties));

    if (context.MethodParameter is Biologic.MethodParameters.RunGEIS methodParameter)
    {
      _channelIndex = Convert.ToByte(methodParameter.ChannelIndex);
      _initialFrequency_Hz = methodParameter.InitialFrequency_Hz;
      _finalFrequency_Hz = methodParameter.FinalFrequency_Hz;
      _frequencyPoints = methodParameter.FrequencyPoints;
      _dcCurrent_A = methodParameter.DcCurrent_A;
      _acAmplitude_A = methodParameter.AcAmplitude_A;
      _outputFile = methodParameter.OutputFile;
    }
    else
    {
      _channelIndex = Convert.ToByte(Properties.GetValueOrDefault("ChannelIndex") ?? 0);
      _initialFrequency_Hz = Convert.ToSingle(Properties.GetValueOrDefault("InitialFrequency_Hz") ?? 100000);
      _finalFrequency_Hz = Convert.ToSingle(Properties.GetValueOrDefault("FinalFrequency_Hz") ?? 0.01);
      _frequencyPoints = Convert.ToInt32(Properties.GetValueOrDefault("FrequencyPoints") ?? 10);
      _dcCurrent_A = Convert.ToSingle(Properties.GetValueOrDefault("DcCurrent_A") ?? 0.0);
      _acAmplitude_A = Convert.ToSingle(Properties.GetValueOrDefault("AcAmplitude_A") ?? 0.0001);
      _outputFile = Properties.GetValueOrDefault("OutputFile")?.ToString();
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

    var channelInfo = ECLibApi.GetChannelInfo(_deviceId, _channelIndex + 1);
    if (channelInfo.Zboard == 0)
    {
      throw new InvalidOperationException($"Channel {_channelIndex} does not support impedance techniques (Zboard == 0)");
    }

    var currentValues = ECLibApi.GetCurrentValues(_deviceId, _channelIndex + 1);
    Log.Information(
      "Channel {Channel} live ranges before GEIS: Ewe={Ewe}V, ERange=[{Min}V, {Max}V], I={Current}A, IRange={IRange}",
      _channelIndex,
      currentValues.Ewe,
      currentValues.EweRangeMin,
      currentValues.EweRangeMax,
      currentValues.I,
      currentValues.IRange);

    _parameters = this.BuildGeisParameters(
      currentStep: _dcCurrent_A,
      amplitudeCurrent: _acAmplitude_A,
      initialFrequency: _initialFrequency_Hz,
      finalFrequency: _finalFrequency_Hz,
      frequencyNumber: _frequencyPoints,
      iRange: SelectCurrentRange(Math.Abs(_dcCurrent_A) + Math.Abs(_acAmplitude_A)));
  }

  public override Sequence.ResultTypes Execute(Sequence.StateContext context)
  {
    try
    {
      Log.Information("Starting GEIS on channel {Channel}: {InitFreq}Hz to {FinalFreq}Hz, {Points} points",
        _channelIndex, _initialFrequency_Hz, _finalFrequency_Hz, _frequencyPoints);

      if (_device == null || !_device.IsConnected || _device.DeviceId < 0)
      {
        throw new InvalidOperationException("Device is not connected");
      }

      this.EnsureChannelStoppedBeforeLoading();

      this.LoadGeisTechniqueWithFallbacks("geis.ecc");

      ECLibApi.StartChannel(_deviceId, _channelIndex + 1);

      Log.Information("GEIS started successfully on channel {Channel}", _channelIndex);

      uint boardType = (uint)ECLibApi.GetChannelBoardType(_deviceId, _channelIndex + 1);
      var processZeroPoints = new List<object>();
      var spectrumPoints = CollectSpectrumPoints(context, boardType, processZeroPoints);

      if (spectrumPoints.Count == 0)
      {
        context.ResultParameter = new
        {
          Success = false,
          Message = $"GEIS completed on channel {_channelIndex}, but no process 1 spectrum points were returned by BL_GetData.",
          ChannelIndex = _channelIndex,
          WarmupPoints = processZeroPoints
        };

        return Sequence.ResultTypes.Error;
      }

      if (!string.IsNullOrWhiteSpace(_outputFile))
      {
        WriteSpectrumCsv(_outputFile!, spectrumPoints);
      }

      context.ResultParameter = new
      {
        Success = true,
        Message = $"GEIS completed on channel {_channelIndex}",
        ChannelIndex = _channelIndex,
        InitialFrequency_Hz = _initialFrequency_Hz,
        FinalFrequency_Hz = _finalFrequency_Hz,
        FrequencyPoints = _frequencyPoints,
        DcCurrent_A = _dcCurrent_A,
        AcAmplitude_A = _acAmplitude_A,
        SpectrumPointCount = spectrumPoints.Count,
        Spectrum = spectrumPoints,
        WarmupPoints = processZeroPoints,
        OutputFile = _outputFile
      };

      return Sequence.ResultTypes.Next;
    }
    catch (ECLibException ex)
    {
      Log.Error("GEIS failed: {ErrorMessage} (Code: {ErrorCode})", ex.Message, ex.ErrorCode);

      context.ResultParameter = new
      {
        Success = false,
        Message = $"Failed to start GEIS: {ex.Message}"
      };

      return Sequence.ResultTypes.Error;
    }
    catch (Exception ex)
    {
      Log.Error(ex, "Unexpected error in GEIS");

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

  private List<GeisSpectrumPoint> CollectSpectrumPoints(
    Sequence.StateContext context,
    uint boardType,
    List<object> processZeroPoints)
  {
    var spectrumPoints = new List<GeisSpectrumPoint>();
    var seenKeys = new HashSet<string>(StringComparer.Ordinal);
    DateTime deadline = DateTime.UtcNow + DefaultTimeout;
    bool stopObserved = false;

    while (DateTime.UtcNow < deadline)
    {
      context.Token.ThrowIfCancellationRequested();

      var currentValues = ECLibApi.GetCurrentValues(_deviceId, _channelIndex + 1);
      stopObserved = (PROG_STATE)currentValues.State == PROG_STATE.STOP;

      var (latestValues, dataInfo, dataBuffer) = ECLibApi.GetData(_deviceId, _channelIndex + 1);
      ParseDataBuffer(latestValues, dataInfo, dataBuffer, boardType, seenKeys, processZeroPoints, spectrumPoints);

      if (stopObserved)
      {
        break;
      }

      Thread.Sleep(DefaultPollInterval);
    }

    if (!stopObserved)
    {
      throw new TimeoutException($"GEIS did not finish within {DefaultTimeout.TotalMinutes:F1} minutes on channel {_channelIndex}.");
    }

    return spectrumPoints;
  }

  private void ParseDataBuffer(
    CurrentValues currentValues,
    DataInfo dataInfo,
    uint[] dataBuffer,
    uint boardType,
    HashSet<string> seenKeys,
    List<object> processZeroPoints,
    List<GeisSpectrumPoint> spectrumPoints)
  {
    if (dataInfo.NbRows <= 0 || dataInfo.NbCols <= 0)
    {
      return;
    }

    if (dataInfo.TechniqueID != (int)TechniqueId.GEIS && dataInfo.TechniqueID != (int)TechniqueId.PEIS)
    {
      return;
    }

    int totalWords = dataInfo.NbRows * dataInfo.NbCols;
    if (totalWords > dataBuffer.Length)
    {
      totalWords = dataBuffer.Length;
    }

    for (int rowIndex = 0; rowIndex < dataInfo.NbRows; rowIndex++)
    {
      int offset = rowIndex * dataInfo.NbCols;
      if (offset + dataInfo.NbCols > totalWords)
      {
        break;
      }

      if (dataInfo.ProcessIndex == 0)
      {
        if (dataInfo.NbCols < 5)
        {
          continue;
        }

        string key = $"p0:{dataBuffer[offset + 1]}:{dataBuffer[offset + 2]}";
        if (!seenKeys.Add(key))
        {
          continue;
        }

        double timeSeconds = ECLibApi.ConvertTimeChannelNumericIntoSeconds(
          dataBuffer[offset + 1],
          dataBuffer[offset + 2],
          currentValues.TimeBase,
          boardType);

        float ewe = ECLibApi.ConvertChannelNumericIntoSingle(dataBuffer[offset + 3], boardType);
        float processZeroCurrent = ECLibApi.ConvertChannelNumericIntoSingle(dataBuffer[offset + 4], boardType);

        processZeroPoints.Add(new
        {
          ProcessIndex = 0,
          Time_s = timeSeconds,
          Potential_V = ewe,
          Current_A = processZeroCurrent
        });

        continue;
      }

      if (dataInfo.ProcessIndex != 1)
      {
        continue;
      }

      if (dataInfo.NbCols < 15)
      {
        continue;
      }

      string spectrumKey = $"p1:{dataBuffer[offset + 1]}:{dataBuffer[offset + 14]}";
      if (!seenKeys.Add(spectrumKey))
      {
        continue;
      }

      float frequency = ECLibApi.ConvertChannelNumericIntoSingle(dataBuffer[offset + 1], boardType);
      float potential = ECLibApi.ConvertChannelNumericIntoSingle(dataBuffer[offset + 5], boardType);
      float current = ECLibApi.ConvertChannelNumericIntoSingle(dataBuffer[offset + 6], boardType);
      float time = ECLibApi.ConvertChannelNumericIntoSingle(dataBuffer[offset + 14], boardType);

      spectrumPoints.Add(new GeisSpectrumPoint
      {
        Frequency_Hz = frequency,
        Time_s = time,
        Potential_V = potential,
        Current_A = current,
        Impedance_Ohm = Math.Abs(current) > float.Epsilon ? Math.Abs(potential / current) : null,
        RawData = dataBuffer.Skip(offset).Take(dataInfo.NbCols).ToArray()
      });
    }
  }

  private static int SelectCurrentRange(float requestedCurrentA)
  {
    float safeCurrent = Math.Max(requestedCurrentA * 2.0f, 100e-12f);

    return safeCurrent switch
    {
      <= 100e-12f => (int)I_RANGE.I_RANGE_100pA,
      <= 1e-9f => (int)I_RANGE.I_RANGE_1nA,
      <= 10e-9f => (int)I_RANGE.I_RANGE_10nA,
      <= 100e-9f => (int)I_RANGE.I_RANGE_100nA,
      <= 1e-6f => (int)I_RANGE.I_RANGE_1uA,
      <= 10e-6f => (int)I_RANGE.I_RANGE_10uA,
      <= 100e-6f => (int)I_RANGE.I_RANGE_100uA,
      <= 1e-3f => (int)I_RANGE.I_RANGE_1mA,
      <= 10e-3f => (int)I_RANGE.I_RANGE_10mA,
      <= 100e-3f => (int)I_RANGE.I_RANGE_100mA,
      <= 1.0f => (int)I_RANGE.I_RANGE_1A,
      _ => (int)I_RANGE.I_RANGE_BOOSTER
    };
  }

  private static void WriteSpectrumCsv(string outputFile, IEnumerable<GeisSpectrumPoint> spectrumPoints)
  {
    string resolvedPath = Path.IsPathRooted(outputFile)
      ? outputFile
      : Path.Combine(Directory.GetCurrentDirectory(), outputFile);

    string? directory = Path.GetDirectoryName(resolvedPath);
    if (!string.IsNullOrWhiteSpace(directory))
    {
      Directory.CreateDirectory(directory);
    }

    var lines = new List<string>
    {
      "Frequency_Hz,Impedance_Ohm,Potential_V,Current_A,Time_s"
    };

    lines.AddRange(spectrumPoints.Select(point => string.Join(",",
      point.Frequency_Hz.ToString("G9", System.Globalization.CultureInfo.InvariantCulture),
      point.Impedance_Ohm?.ToString("G9", System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
      point.Potential_V.ToString("G9", System.Globalization.CultureInfo.InvariantCulture),
      point.Current_A.ToString("G9", System.Globalization.CultureInfo.InvariantCulture),
      point.Time_s.ToString("G9", System.Globalization.CultureInfo.InvariantCulture))));

    File.WriteAllLines(resolvedPath, lines, Encoding.UTF8);
  }

  private EccParam[] BuildEccParamArray(Dictionary<string, object> parameters)
  {
    var paramList = new List<EccParam>();

    if (parameters.ContainsKey("vs_initializer"))
    {
      paramList.Add(ECLibApi.DefineBoolParameter("vs_initializer", Convert.ToBoolean(parameters["vs_initializer"]), 0));
    }

    if (parameters.ContainsKey("Initial_Current_step"))
    {
      paramList.Add(ECLibApi.DefineSingleParameter("Initial_Current_step", Convert.ToSingle(parameters["Initial_Current_step"]), 0));
    }

    if (parameters.ContainsKey("Duration_step"))
    {
      paramList.Add(ECLibApi.DefineSingleParameter("Duration_step", Convert.ToSingle(parameters["Duration_step"]), 0));
    }

    if (parameters.ContainsKey("Record EVERY_dT"))
    {
      paramList.Add(ECLibApi.DefineSingleParameter("Record EVERY_dT", Convert.ToSingle(parameters["Record EVERY_dT"]), 0));
    }

    if (parameters.ContainsKey("Record_every_dT"))
    {
      paramList.Add(ECLibApi.DefineSingleParameter("Record_every_dT", Convert.ToSingle(parameters["Record_every_dT"]), 0));
    }

    if (parameters.ContainsKey("Record EVERY_dE"))
    {
      paramList.Add(ECLibApi.DefineSingleParameter("Record EVERY_dE", Convert.ToSingle(parameters["Record EVERY_dE"]), 0));
    }

    if (parameters.ContainsKey("Record_every_dE"))
    {
      paramList.Add(ECLibApi.DefineSingleParameter("Record_every_dE", Convert.ToSingle(parameters["Record_every_dE"]), 0));
    }

    if (parameters.ContainsKey("Final_frequency"))
    {
      paramList.Add(ECLibApi.DefineSingleParameter("Final_frequency", Convert.ToSingle(parameters["Final_frequency"]), 0));
    }

    if (parameters.ContainsKey("Final_freqency"))
    {
      paramList.Add(ECLibApi.DefineSingleParameter("Final_freqency", Convert.ToSingle(parameters["Final_freqency"]), 0));
    }

    if (parameters.ContainsKey("Initial_frequency"))
    {
      paramList.Add(ECLibApi.DefineSingleParameter("Initial_frequency", Convert.ToSingle(parameters["Initial_frequency"]), 0));
    }

    if (parameters.ContainsKey("Initial_freqency"))
    {
      paramList.Add(ECLibApi.DefineSingleParameter("Initial_freqency", Convert.ToSingle(parameters["Initial_freqency"]), 0));
    }

    if (parameters.ContainsKey("sweep"))
    {
      paramList.Add(ECLibApi.DefineBoolParameter("sweep", Convert.ToBoolean(parameters["sweep"]), 0));
    }

    if (parameters.ContainsKey("Amplitude_Current"))
    {
      paramList.Add(ECLibApi.DefineSingleParameter("Amplitude_Current", Convert.ToSingle(parameters["Amplitude_Current"]), 0));
    }

    if (parameters.ContainsKey("Frequency_number"))
    {
      paramList.Add(ECLibApi.DefineIntParameter("Frequency_number", Convert.ToInt32(parameters["Frequency_number"]), 0));
    }

    if (parameters.ContainsKey("Average_N_times"))
    {
      paramList.Add(ECLibApi.DefineIntParameter("Average_N_times", Convert.ToInt32(parameters["Average_N_times"]), 0));
    }

    if (parameters.ContainsKey("Average_N(times"))
    {
      paramList.Add(ECLibApi.DefineIntParameter("Average_N(times", Convert.ToInt32(parameters["Average_N(times"]), 0));
    }

    if (parameters.ContainsKey("Average_N(times)"))
    {
      paramList.Add(ECLibApi.DefineIntParameter("Average_N(times)", Convert.ToInt32(parameters["Average_N(times)"]), 0));
    }

    if (parameters.ContainsKey("Correction"))
    {
      paramList.Add(ECLibApi.DefineBoolParameter("Correction", Convert.ToBoolean(parameters["Correction"]), 0));
    }

    if (parameters.ContainsKey("Wait_for_steady"))
    {
      paramList.Add(ECLibApi.DefineSingleParameter("Wait_for_steady", Convert.ToSingle(parameters["Wait_for_steady"]), 0));
    }

    if (parameters.ContainsKey("I_Range"))
    {
      paramList.Add(ECLibApi.DefineIntParameter("I_Range", Convert.ToInt32(parameters["I_Range"]), 0));
    }

    if (parameters.ContainsKey("E_Range"))
    {
      paramList.Add(ECLibApi.DefineIntParameter("E_Range", Convert.ToInt32(parameters["E_Range"]), 0));
    }

    if (parameters.ContainsKey("vs_initial"))
    {
      paramList.Add(ECLibApi.DefineBoolParameter("vs_initial", Convert.ToBoolean(parameters["vs_initial"]), 0));
    }

    if (parameters.ContainsKey("Is_initial"))
    {
      paramList.Add(ECLibApi.DefineSingleParameter("Is_initial", Convert.ToSingle(parameters["Is_initial"]), 0));
    }

    if (parameters.ContainsKey("Is_final"))
    {
      paramList.Add(ECLibApi.DefineSingleParameter("Is_final", Convert.ToSingle(parameters["Is_final"]), 0));
    }

    if (parameters.ContainsKey("Duration"))
    {
      paramList.Add(ECLibApi.DefineSingleParameter("Duration", Convert.ToSingle(parameters["Duration"]), 0));
    }

    if (parameters.ContainsKey("Ia"))
    {
      paramList.Add(ECLibApi.DefineSingleParameter("Ia", Convert.ToSingle(parameters["Ia"]), 0));
    }

    if (parameters.ContainsKey("freq_init"))
    {
      paramList.Add(ECLibApi.DefineSingleParameter("freq_init", Convert.ToSingle(parameters["freq_init"]), 0));
    }

    if (parameters.ContainsKey("freq_final"))
    {
      paramList.Add(ECLibApi.DefineSingleParameter("freq_final", Convert.ToSingle(parameters["freq_final"]), 0));
    }

    if (parameters.ContainsKey("Points_per_decade"))
    {
      paramList.Add(ECLibApi.DefineIntParameter("Points_per_decade", Convert.ToInt32(parameters["Points_per_decade"]), 0));
    }

    return paramList.ToArray();
  }

  private void LoadGeisTechniqueWithFallbacks(string preferredTechniqueFile)
  {
    string eccPath = _device!.ResolveTechniqueFilePath(preferredTechniqueFile, _channelIndex);
    if (!File.Exists(eccPath))
    {
      throw new FileNotFoundException($"GEIS technique file not found: {eccPath}", eccPath);
    }

    string nativeEccPath = _device.GetNativeTechniqueFilePath(preferredTechniqueFile, _channelIndex);
    this.ResetChannelBeforeGeisAttempt();

    var eccParams = new EccParams
    {
      len = 0,
      pParams = IntPtr.Zero
    };

    try
    {
      var paramArray = BuildEccParamArray(_parameters ?? this.BuildGeisParameters(
        currentStep: _dcCurrent_A,
        amplitudeCurrent: _acAmplitude_A,
        initialFrequency: _initialFrequency_Hz,
        finalFrequency: _finalFrequency_Hz,
        frequencyNumber: _frequencyPoints,
        iRange: SelectCurrentRange(Math.Abs(_dcCurrent_A) + Math.Abs(_acAmplitude_A))));
      eccParams.len = paramArray.Length;

      int paramSize = System.Runtime.InteropServices.Marshal.SizeOf<EccParam>();
      eccParams.pParams = System.Runtime.InteropServices.Marshal.AllocHGlobal(paramSize * paramArray.Length);
      for (int i = 0; i < paramArray.Length; i++)
      {
        IntPtr ptr = IntPtr.Add(eccParams.pParams, i * paramSize);
        System.Runtime.InteropServices.Marshal.StructureToPtr(paramArray[i], ptr, false);
      }

      ECLibApi.LoadTechnique(
        _deviceId,
        _channelIndex + 1,
        nativeEccPath,
        ref eccParams,
        first: true,
        last: true,
        display: false);
    }
    finally
    {
      if (eccParams.pParams != IntPtr.Zero)
      {
        System.Runtime.InteropServices.Marshal.FreeHGlobal(eccParams.pParams);
      }
    }
  }

  private Dictionary<string, object> BuildGeisParameters(
    float currentStep,
    float amplitudeCurrent,
    float initialFrequency,
    float finalFrequency,
    int frequencyNumber,
    int iRange,
    bool sweep = false)
  {
    var parameters = new Dictionary<string, object>
    {
      ["vs_initial"] = false,
      ["Initial_Current_step"] = currentStep,
      ["Duration_step"] = Convert.ToSingle(Properties?.GetValueOrDefault("Duration_step") ?? DefaultDurationStep),
      ["Record_every_dT"] = Convert.ToSingle(Properties?.GetValueOrDefault("Record_every_dT") ?? DefaultRecordEveryDt),
      ["Record_every_dE"] = Convert.ToSingle(Properties?.GetValueOrDefault("Record_every_dE") ?? DefaultRecordEveryDe),
      ["Final_frequency"] = finalFrequency,
      ["Initial_frequency"] = initialFrequency,
      ["sweep"] = sweep,
      ["Amplitude_Current"] = amplitudeCurrent,
      ["Frequency_number"] = frequencyNumber,
      ["Average_N_times"] = Convert.ToInt32(Properties?.GetValueOrDefault("Average_N_times") ?? DefaultAverageCount),
      ["Correction"] = Convert.ToBoolean(Properties?.GetValueOrDefault("Correction") ?? false),
      ["Wait_for_steady"] = Convert.ToSingle(Properties?.GetValueOrDefault("Wait_for_steady") ?? DefaultWaitForSteady),
      ["I_Range"] = iRange
    };

    return parameters;
  }

  private void EnsureChannelStoppedBeforeLoading()
  {
    var currentValues = ECLibApi.GetCurrentValues(_deviceId, _channelIndex + 1);
    if ((PROG_STATE)currentValues.State == PROG_STATE.STOP)
    {
      return;
    }

    Log.Information("Stopping channel {Channel} before loading GEIS because current state is {State}", _channelIndex, (PROG_STATE)currentValues.State);
    ECLibApi.StopChannel(_deviceId, _channelIndex + 1);

    DateTime deadline = DateTime.UtcNow + StopWaitTimeout;
    while (DateTime.UtcNow < deadline)
    {
      currentValues = ECLibApi.GetCurrentValues(_deviceId, _channelIndex + 1);
      if ((PROG_STATE)currentValues.State == PROG_STATE.STOP)
      {
        return;
      }

      Thread.Sleep(TimeSpan.FromMilliseconds(100));
    }

    throw new TimeoutException($"Channel {_channelIndex} did not reach STOP before GEIS loading.");
  }

  private void ResetChannelBeforeGeisAttempt()
  {
    if (_device == null)
    {
      throw new InvalidOperationException("GEIS device context is not available for channel reset.");
    }

    bool resetSucceeded = _device.ResetChannelTechniqueState(_channelIndex, forceFirmwareReload: true);
    if (!resetSucceeded)
    {
      throw new InvalidOperationException($"Failed to reset channel {_channelIndex} before GEIS attempt.");
    }
  }

  private sealed class GeisSpectrumPoint
  {
    public float Frequency_Hz { get; init; }
    public float Potential_V { get; init; }
    public float Current_A { get; init; }
    public float Time_s { get; init; }
    public float? Impedance_Ohm { get; init; }
    public uint[] RawData { get; init; } = [];
  }
}
