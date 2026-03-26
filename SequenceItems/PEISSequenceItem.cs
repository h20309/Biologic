using Biologic.Devices;
using Biologic.Native;
using DeviceControlSoftware;
using Serilog;
using System.Text;

namespace Biologic.SequenceItems;

/// <summary>
/// Potentiostatic Electrochemical Impedance Spectroscopy (PEIS)
/// Applies AC voltage and measures impedance at different frequencies.
/// </summary>
public class PEISSequenceItem : SequenceItem
{
  private const float DefaultRecordEveryDt = 0.0f;
  private const float DefaultRecordEveryDi = 0.0f;
  private const float DefaultWaitForSteady = 0.0f;
  private const float DefaultDurationStep = 1.0f;
  private const int DefaultAverageCount = 1;
  private static readonly TimeSpan DefaultPollInterval = TimeSpan.FromMilliseconds(200);
  private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(10);
  private static readonly TimeSpan StopDrainTimeout = TimeSpan.FromSeconds(10);
  private const int StopDrainEmptyReadThreshold = 3;

  private int _deviceId;
  private byte _channelIndex;
  private float _initialFrequency_Hz;
  private float _finalFrequency_Hz;
  private int _frequencyPoints;
  private float _dcVoltage_V;
  private float _acAmplitude_V;
  private string? _outputFile;
  private Dictionary<string, object>? _parameters;
  private ECLabDevice? _device;
  private string? _parameterSummary;

  public override void Initialize(Sequence.StateContext context)
  {
    if (Properties == null)
      throw new ArgumentNullException(nameof(Properties));

    if (context.MethodParameter is Biologic.MethodParameters.RunPEIS methodParameter)
    {
      _channelIndex = Convert.ToByte(methodParameter.ChannelIndex);
      _initialFrequency_Hz = methodParameter.InitialFrequency_Hz;
      _finalFrequency_Hz = methodParameter.FinalFrequency_Hz;
      _frequencyPoints = methodParameter.FrequencyPoints;
      _dcVoltage_V = methodParameter.DcVoltage_V;
      _acAmplitude_V = methodParameter.AcAmplitude_V;
      Properties["Duration_step"] = methodParameter.Duration_step;
      Properties["Record_every_dT"] = methodParameter.Record_every_dT;
      Properties["Record_every_dI"] = methodParameter.Record_every_dI;
      Properties["Average_N_times"] = methodParameter.Average_N_times;
      Properties["Correction"] = methodParameter.Correction;
      Properties["Wait_for_steady"] = methodParameter.Wait_for_steady;
      Properties["sweep"] = methodParameter.sweep;
      _outputFile = methodParameter.OutputFile;
    }
    else
    {
      _channelIndex = Convert.ToByte(Properties.GetValueOrDefault("ChannelIndex") ?? 0);
      _initialFrequency_Hz = Convert.ToSingle(Properties.GetValueOrDefault("InitialFrequency_Hz") ?? 100000);
      _finalFrequency_Hz = Convert.ToSingle(Properties.GetValueOrDefault("FinalFrequency_Hz") ?? 0.1f);
      _frequencyPoints = Convert.ToInt32(Properties.GetValueOrDefault("FrequencyPoints") ?? 10);
      _dcVoltage_V = Convert.ToSingle(Properties.GetValueOrDefault("DcVoltage_V") ?? 0.0f);
      _acAmplitude_V = Convert.ToSingle(Properties.GetValueOrDefault("AcAmplitude_V") ?? 0.01f);
      _outputFile = Properties.GetValueOrDefault("OutputFile")?.ToString();
    }

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

    _parameters = this.BuildPeisParameters(
      voltageStep: _dcVoltage_V,
      amplitudeVoltage: _acAmplitude_V,
      initialFrequency: _initialFrequency_Hz,
      finalFrequency: _finalFrequency_Hz,
      frequencyNumber: _frequencyPoints);
    _parameterSummary = string.Join(
      ", ",
      _parameters
        .OrderBy(entry => entry.Key, StringComparer.Ordinal)
        .Select(entry => $"{entry.Key}={entry.Value}"));
  }

  public override Sequence.ResultTypes Execute(Sequence.StateContext context)
  {
    ECLabAutomation? automation = context.SequenceDispatcher as ECLabAutomation;

    try
    {
      Log.Information("Starting PEIS on channel {Channel}: {InitFreq}Hz to {FinalFreq}Hz, {Points} points",
        _channelIndex, _initialFrequency_Hz, _finalFrequency_Hz, _frequencyPoints);
      Log.Information("Resolved PEIS parameters for channel {Channel}: {Parameters}", _channelIndex, _parameterSummary);

      if (_device == null || !_device.IsConnected || _device.DeviceId < 0)
      {
        throw new InvalidOperationException("Device is not connected");
      }

      this.EnsureChannelReadyForNewSequence();
      this.LoadPeisTechnique("peis.ecc");

      ECLibApi.StartChannel(_deviceId, _channelIndex + 1);

      Log.Information("PEIS started successfully on channel {Channel}", _channelIndex);

      uint boardType = (uint)ECLibApi.GetChannelBoardType(_deviceId, _channelIndex + 1);
      var processZeroPoints = new List<object>();
      var spectrumPoints = CollectSpectrumPoints(context, boardType, processZeroPoints);

      if (spectrumPoints.Count == 0)
      {
        Log.Warning(
          "PEIS completed on channel {Channel} but no process 1 spectrum points were captured. WarmupPoints={WarmupPointCount}",
          _channelIndex,
          processZeroPoints.Count);

        context.ResultParameter = new
        {
          Success = false,
          Message = $"PEIS completed on channel {_channelIndex}, but no process 1 spectrum points were returned by BL_GetData.",
          ChannelIndex = _channelIndex,
          WarmupPoints = processZeroPoints
        };

        return Sequence.ResultTypes.Error;
      }

      if (!string.IsNullOrWhiteSpace(_outputFile))
      {
        WriteSpectrumCsv(_outputFile!, spectrumPoints);
      }

      var fullResult = new
      {
        Success = true,
        Message = $"PEIS completed on channel {_channelIndex}",
        ChannelIndex = _channelIndex,
        InitialFrequency_Hz = _initialFrequency_Hz,
        FinalFrequency_Hz = _finalFrequency_Hz,
        FrequencyPoints = _frequencyPoints,
        DcVoltage_V = _dcVoltage_V,
        AcAmplitude_V = _acAmplitude_V,
        SpectrumPointCount = spectrumPoints.Count,
        Spectrum = spectrumPoints,
        WarmupPoints = processZeroPoints,
        OutputFile = _outputFile
      };

      if (automation != null)
      {
        string fullResultJson = SequenceDispatcher.Serialize(fullResult);
        automation.PublishEisResultPayload(context.ID, "RunPEIS", DateTime.UtcNow, fullResultJson);
      }

      context.ResultParameter = new
      {
        Success = true,
        Message = $"PEIS completed on channel {_channelIndex}",
        ChannelIndex = _channelIndex,
        InitialFrequency_Hz = _initialFrequency_Hz,
        FinalFrequency_Hz = _finalFrequency_Hz,
        FrequencyPoints = _frequencyPoints,
        DcVoltage_V = _dcVoltage_V,
        AcAmplitude_V = _acAmplitude_V,
        SpectrumPointCount = spectrumPoints.Count,
        OutputFile = _outputFile,
        ResultLocation = "LatestEISResultJson / LatestEISHistoryJson",
        MethodResponseMode = "SummaryOnly"
      };

      return Sequence.ResultTypes.Next;
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
    finally
    {
      // Polling uses only BL_GetCurrentValues (non-destructive) so it can
      // remain active during PEIS without consuming BL_GetData results.
    }
  }

  public override void Deinitialize(Sequence.StateContext context)
  {
  }

  private List<PeisSpectrumPoint> CollectSpectrumPoints(
    Sequence.StateContext context,
    uint boardType,
    List<object> processZeroPoints)
  {
    var spectrumPoints = new List<PeisSpectrumPoint>();
    var seenKeys = new HashSet<string>(StringComparer.Ordinal);
    DateTime deadline = DateTime.UtcNow + DefaultTimeout;
    bool stopObserved = false;
    DateTime? stopDrainDeadline = null;
    int emptyReadsAfterStop = 0;

    while (DateTime.UtcNow < deadline)
    {
      context.Token.ThrowIfCancellationRequested();

      var currentValues = ECLibApi.GetCurrentValues(_deviceId, _channelIndex + 1);
      bool stopNow = (PROG_STATE)currentValues.State == PROG_STATE.STOP;
      if (stopNow && !stopObserved)
      {
        stopObserved = true;
        stopDrainDeadline = DateTime.UtcNow + StopDrainTimeout;
        emptyReadsAfterStop = 0;
        Log.Information(
          "PEIS channel {Channel} entered STOP state. Continuing to drain BL_GetData for up to {DrainSeconds:F1}s.",
          _channelIndex,
          StopDrainTimeout.TotalSeconds);
      }

      var (latestValues, dataInfo, dataBuffer) = ECLibApi.GetData(_deviceId, _channelIndex + 1);
      ParseDataBuffer(latestValues, dataInfo, dataBuffer, boardType, seenKeys, processZeroPoints, spectrumPoints);

      if (stopObserved)
      {
        if (dataInfo.NbRows <= 0)
        {
          emptyReadsAfterStop++;
        }
        else
        {
          emptyReadsAfterStop = 0;
          stopDrainDeadline = DateTime.UtcNow + StopDrainTimeout;
        }

        if (stopDrainDeadline.HasValue &&
            DateTime.UtcNow >= stopDrainDeadline.Value &&
            emptyReadsAfterStop >= StopDrainEmptyReadThreshold)
        {
          break;
        }
      }

      Thread.Sleep(DefaultPollInterval);
    }

    if (!stopObserved)
    {
      throw new TimeoutException($"PEIS did not finish within {DefaultTimeout.TotalMinutes:F1} minutes on channel {_channelIndex}.");
    }

    Log.Information("PEIS collection completed on channel {Channel} with {PointCount} spectrum points.", _channelIndex, spectrumPoints.Count);

    return spectrumPoints;
  }

  private void ParseDataBuffer(
    CurrentValues currentValues,
    DataInfo dataInfo,
    uint[] dataBuffer,
    uint boardType,
    HashSet<string> seenKeys,
    List<object> processZeroPoints,
    List<PeisSpectrumPoint> spectrumPoints)
  {
    if (dataInfo.NbRows <= 0 || dataInfo.NbCols <= 0)
    {
      return;
    }

    if (dataInfo.TechniqueID != (int)TechniqueId.PEIS)
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

      if (dataInfo.ProcessIndex != 1 || dataInfo.NbCols < 14)
      {
        continue;
      }

      string spectrumKey = $"p1:{dataBuffer[offset + 0]}:{dataBuffer[offset + 13]}";
      if (!seenKeys.Add(spectrumKey))
      {
        continue;
      }

      float[] convertedColumns = new float[dataInfo.NbCols];
      for (int columnIndex = 0; columnIndex < dataInfo.NbCols; columnIndex++)
      {
        if (dataInfo.NbCols == 15 && columnIndex == 14)
        {
          convertedColumns[columnIndex] = dataBuffer[offset + columnIndex];
          continue;
        }

        convertedColumns[columnIndex] = ECLibApi.ConvertChannelNumericIntoSingle(dataBuffer[offset + columnIndex], boardType);
      }

      float frequency = convertedColumns[0];
      float potentialAmplitude = convertedColumns[1];
      float currentAmplitude = convertedColumns[2];
      float phaseZwe = convertedColumns[3];
      float potential = convertedColumns[4];
      float current = convertedColumns[5];
      float counterPotentialAmplitude = convertedColumns[7];
      float counterCurrentAmplitude = convertedColumns[8];
      float phaseZce = convertedColumns[9];
      float counterPotential = convertedColumns[10];
      float time = convertedColumns[13];
      int? rangeCode = dataInfo.NbCols > 14 ? (int)dataBuffer[offset + 14] : null;
      float? impedance = ECLibApi.CalculateImpedanceMagnitude(potentialAmplitude, currentAmplitude);
      ImpedanceComponents? workingImpedanceComponents = ECLibApi.CalculateImpedanceComponents(impedance, phaseZwe);
      float? counterImpedance = ECLibApi.CalculateImpedanceMagnitude(counterPotentialAmplitude, counterCurrentAmplitude);
      ImpedanceComponents? counterImpedanceComponents = ECLibApi.CalculateImpedanceComponents(counterImpedance, phaseZce);

      spectrumPoints.Add(new PeisSpectrumPoint
      {
        Frequency_Hz = frequency,
        Time_s = time,
        Potential_V = potential,
        Current_A = current,
        PotentialAmplitude_V = potentialAmplitude,
        CurrentAmplitude_A = currentAmplitude,
        CounterPotential_V = counterPotential,
        CounterPotentialAmplitude_V = counterPotentialAmplitude,
        CounterCurrentAmplitude_A = counterCurrentAmplitude,
        PhaseZwe = phaseZwe,
        PhaseZce = phaseZce,
        RangeCode = rangeCode,
        Impedance_Ohm = impedance,
        ImpedanceReal_Ohm = workingImpedanceComponents?.Real_Ohm,
        ImpedanceImaginary_Ohm = workingImpedanceComponents?.Imaginary_Ohm,
        NyquistImaginary_Ohm = workingImpedanceComponents?.NyquistImaginary_Ohm,
        CounterImpedance_Ohm = counterImpedance,
        CounterImpedanceReal_Ohm = counterImpedanceComponents?.Real_Ohm,
        CounterImpedanceImaginary_Ohm = counterImpedanceComponents?.Imaginary_Ohm,
        RawData = dataBuffer.Skip(offset).Take(dataInfo.NbCols).ToArray()
      });
    }
  }

  private static void WriteSpectrumCsv(string outputFile, IEnumerable<PeisSpectrumPoint> spectrumPoints)
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
      "Frequency_Hz,Impedance_Ohm,ImpedanceReal_Ohm,ImpedanceImaginary_Ohm,NyquistImaginary_Ohm,Potential_V,Current_A,PotentialAmplitude_V,CurrentAmplitude_A,CounterPotential_V,CounterPotentialAmplitude_V,CounterCurrentAmplitude_A,CounterImpedance_Ohm,CounterImpedanceReal_Ohm,CounterImpedanceImaginary_Ohm,PhaseZwe,PhaseZce,Time_s,RangeCode"
    };

    lines.AddRange(spectrumPoints.Select(point => string.Join(",",
      point.Frequency_Hz.ToString("G9", System.Globalization.CultureInfo.InvariantCulture),
      point.Impedance_Ohm?.ToString("G9", System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
      point.ImpedanceReal_Ohm?.ToString("G9", System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
      point.ImpedanceImaginary_Ohm?.ToString("G9", System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
      point.NyquistImaginary_Ohm?.ToString("G9", System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
      point.Potential_V.ToString("G9", System.Globalization.CultureInfo.InvariantCulture),
      point.Current_A.ToString("G9", System.Globalization.CultureInfo.InvariantCulture),
      point.PotentialAmplitude_V.ToString("G9", System.Globalization.CultureInfo.InvariantCulture),
      point.CurrentAmplitude_A.ToString("G9", System.Globalization.CultureInfo.InvariantCulture),
      point.CounterPotential_V.ToString("G9", System.Globalization.CultureInfo.InvariantCulture),
      point.CounterPotentialAmplitude_V.ToString("G9", System.Globalization.CultureInfo.InvariantCulture),
      point.CounterCurrentAmplitude_A.ToString("G9", System.Globalization.CultureInfo.InvariantCulture),
      point.CounterImpedance_Ohm?.ToString("G9", System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
      point.CounterImpedanceReal_Ohm?.ToString("G9", System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
      point.CounterImpedanceImaginary_Ohm?.ToString("G9", System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
      point.PhaseZwe.ToString("G9", System.Globalization.CultureInfo.InvariantCulture),
      point.PhaseZce.ToString("G9", System.Globalization.CultureInfo.InvariantCulture),
      point.Time_s.ToString("G9", System.Globalization.CultureInfo.InvariantCulture),
      point.RangeCode?.ToString() ?? string.Empty)));

    File.WriteAllLines(resolvedPath, lines, Encoding.UTF8);
  }

  private EccParam[] BuildEccParamArray(Dictionary<string, object> parameters)
  {
    var paramList = new List<EccParam>();

    if (parameters.ContainsKey("Initial_Voltage_step"))
    {
      paramList.Add(ECLibApi.DefineSingleParameter("Initial_Voltage_step", Convert.ToSingle(parameters["Initial_Voltage_step"]), 0));
    }

    if (parameters.ContainsKey("Duration_step"))
    {
      paramList.Add(ECLibApi.DefineSingleParameter("Duration_step", Convert.ToSingle(parameters["Duration_step"]), 0));
    }

    if (parameters.ContainsKey("Record_every_dT"))
    {
      paramList.Add(ECLibApi.DefineSingleParameter("Record_every_dT", Convert.ToSingle(parameters["Record_every_dT"]), 0));
    }

    if (parameters.ContainsKey("Record_every_dI"))
    {
      paramList.Add(ECLibApi.DefineSingleParameter("Record_every_dI", Convert.ToSingle(parameters["Record_every_dI"]), 0));
    }

    if (parameters.ContainsKey("Final_frequency"))
    {
      paramList.Add(ECLibApi.DefineSingleParameter("Final_frequency", Convert.ToSingle(parameters["Final_frequency"]), 0));
    }

    if (parameters.ContainsKey("Initial_frequency"))
    {
      paramList.Add(ECLibApi.DefineSingleParameter("Initial_frequency", Convert.ToSingle(parameters["Initial_frequency"]), 0));
    }

    if (parameters.ContainsKey("sweep"))
    {
      paramList.Add(ECLibApi.DefineBoolParameter("sweep", Convert.ToBoolean(parameters["sweep"]), 0));
    }

    if (parameters.ContainsKey("Amplitude_Voltage"))
    {
      paramList.Add(ECLibApi.DefineSingleParameter("Amplitude_Voltage", Convert.ToSingle(parameters["Amplitude_Voltage"]), 0));
    }

    if (parameters.ContainsKey("Frequency_number"))
    {
      paramList.Add(ECLibApi.DefineIntParameter("Frequency_number", Convert.ToInt32(parameters["Frequency_number"]), 0));
    }

    if (parameters.ContainsKey("Average_N_times"))
    {
      paramList.Add(ECLibApi.DefineIntParameter("Average_N_times", Convert.ToInt32(parameters["Average_N_times"]), 0));
    }

    if (parameters.ContainsKey("Correction"))
    {
      paramList.Add(ECLibApi.DefineBoolParameter("Correction", Convert.ToBoolean(parameters["Correction"]), 0));
    }

    if (parameters.ContainsKey("Wait_for_steady"))
    {
      paramList.Add(ECLibApi.DefineSingleParameter("Wait_for_steady", Convert.ToSingle(parameters["Wait_for_steady"]), 0));
    }

    if (parameters.ContainsKey("vs_initial"))
    {
      paramList.Add(ECLibApi.DefineBoolParameter("vs_initial", Convert.ToBoolean(parameters["vs_initial"]), 0));
    }

    return paramList.ToArray();
  }

  private void LoadPeisTechnique(string preferredTechniqueFile)
  {
    string eccPath = _device!.ResolveTechniqueFilePath(preferredTechniqueFile, _channelIndex);
    if (!File.Exists(eccPath))
    {
      throw new FileNotFoundException($"PEIS technique file not found: {eccPath}", eccPath);
    }

    string nativeEccPath = _device.GetNativeTechniqueFilePath(preferredTechniqueFile, _channelIndex);

    var eccParams = new EccParams
    {
      len = 0,
      pParams = IntPtr.Zero
    };

    try
    {
      var paramArray = BuildEccParamArray(_parameters ?? this.BuildPeisParameters(
        voltageStep: _dcVoltage_V,
        amplitudeVoltage: _acAmplitude_V,
        initialFrequency: _initialFrequency_Hz,
        finalFrequency: _finalFrequency_Hz,
        frequencyNumber: _frequencyPoints));
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

  private Dictionary<string, object> BuildPeisParameters(
    float voltageStep,
    float amplitudeVoltage,
    float initialFrequency,
    float finalFrequency,
    int frequencyNumber,
    bool sweep = false)
  {
    return new Dictionary<string, object>
    {
      ["vs_initial"] = GetConfiguredBoolean(false, "vs_initial"),
      ["Initial_Voltage_step"] = voltageStep,
      ["Duration_step"] = GetConfiguredSingle(DefaultDurationStep, "Duration_step"),
      ["Record_every_dT"] = GetConfiguredSingle(DefaultRecordEveryDt, "Record_every_dT"),
      ["Record_every_dI"] = GetConfiguredSingle(DefaultRecordEveryDi, "Record_every_dI"),
      ["Final_frequency"] = finalFrequency,
      ["Initial_frequency"] = initialFrequency,
      ["sweep"] = GetConfiguredBoolean(sweep, "sweep"),
      ["Amplitude_Voltage"] = amplitudeVoltage,
      ["Frequency_number"] = frequencyNumber,
      ["Average_N_times"] = GetConfiguredInt(DefaultAverageCount, "Average_N_times"),
      ["Correction"] = GetConfiguredBoolean(false, "Correction"),
      ["Wait_for_steady"] = GetConfiguredSingle(DefaultWaitForSteady, "Wait_for_steady"),
    };
  }

  private float GetConfiguredSingle(float defaultValue, params string[] keys)
  {
    if (Properties == null)
    {
      return defaultValue;
    }

    foreach (string key in keys)
    {
      if (Properties.TryGetValue(key, out object? value) && value != null)
      {
        return Convert.ToSingle(value);
      }
    }

    return defaultValue;
  }

  private int GetConfiguredInt(int defaultValue, params string[] keys)
  {
    if (Properties == null)
    {
      return defaultValue;
    }

    foreach (string key in keys)
    {
      if (Properties.TryGetValue(key, out object? value) && value != null)
      {
        return Convert.ToInt32(value);
      }
    }

    return defaultValue;
  }

  private bool GetConfiguredBoolean(bool defaultValue, params string[] keys)
  {
    if (Properties == null)
    {
      return defaultValue;
    }

    foreach (string key in keys)
    {
      if (Properties.TryGetValue(key, out object? value) && value != null)
      {
        return Convert.ToBoolean(value);
      }
    }

    return defaultValue;
  }

  private void EnsureChannelReadyForNewSequence()
  {
    if (_device == null)
    {
      throw new InvalidOperationException("PEIS device context is not available.");
    }

    if (!_device.CanStartSequenceOnChannel(_channelIndex, out string message))
    {
      throw new InvalidOperationException(message);
    }
  }

  private sealed class PeisSpectrumPoint
  {
    public float Frequency_Hz { get; init; }
    public float Potential_V { get; init; }
    public float Current_A { get; init; }
    public float PotentialAmplitude_V { get; init; }
    public float CurrentAmplitude_A { get; init; }
    public float CounterPotential_V { get; init; }
    public float CounterPotentialAmplitude_V { get; init; }
    public float CounterCurrentAmplitude_A { get; init; }
    public float PhaseZwe { get; init; }
    public float PhaseZce { get; init; }
    public float Time_s { get; init; }
    public int? RangeCode { get; init; }
    public float? Impedance_Ohm { get; init; }
    public float? ImpedanceReal_Ohm { get; init; }
    public float? ImpedanceImaginary_Ohm { get; init; }
    public float? NyquistImaginary_Ohm { get; init; }
    public float? CounterImpedance_Ohm { get; init; }
    public float? CounterImpedanceReal_Ohm { get; init; }
    public float? CounterImpedanceImaginary_Ohm { get; init; }
    public uint[] RawData { get; init; } = [];
  }
}
