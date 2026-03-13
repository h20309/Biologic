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
  private const float DefaultRecordEveryDt = 0.1f;
  private const float DefaultRecordEveryDe = 0.001f;
  private const float DefaultWaitForSteady = 0.1f;
  private static readonly TimeSpan DefaultPollInterval = TimeSpan.FromMilliseconds(200);
  private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(5);

  private int _deviceId;
  private byte _channelIndex;
  private float _initialFrequency_Hz;
  private float _finalFrequency_Hz;
  private int _frequencyPoints;
  private float _dcCurrent_A;
  private float _acAmplitude_A;
  private string? _outputFile;
  private Dictionary<string, object>? _parameters;

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
    if (device != null && device.GetProperty("DeviceId") is int deviceId)
    {
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

    int currentRange = SelectCurrentRange(Math.Abs(_dcCurrent_A) + Math.Abs(_acAmplitude_A));

    // GEIS parameter labels are case-sensitive and must match the vendor manual.
    _parameters = new Dictionary<string, object>
    {
      ["vs_initializer"] = false,
      ["Initial_Current_step"] = _dcCurrent_A,
      ["Duration_step"] = 0.0f,
      ["Record EVERY_dT"] = DefaultRecordEveryDt,
      ["Record EVERY_dE"] = DefaultRecordEveryDe,
      ["Final_freqency"] = _finalFrequency_Hz,
      ["Initial_freqency"] = _initialFrequency_Hz,
      ["sweep"] = false,
      ["Amplitude_Current"] = _acAmplitude_A,
      ["Frequency_number"] = _frequencyPoints,
      ["Average_N(times"] = 1,
      ["Correction"] = false,
      ["Wait_for_steady"] = DefaultWaitForSteady,
      ["I_Range"] = currentRange
    };
  }

  public override Sequence.ResultTypes Execute(Sequence.StateContext context)
  {
    try
    {
      Log.Information("Starting GEIS on channel {Channel}: {InitFreq}Hz to {FinalFreq}Hz, {Points} points",
        _channelIndex, _initialFrequency_Hz, _finalFrequency_Hz, _frequencyPoints);

      string eccPath = ResolveTechniquePath("geis.ecc");

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

  private static string ResolveTechniquePath(string techniqueFileName)
  {
    string[] candidatePaths =
    [
      Path.Combine(Directory.GetCurrentDirectory(), "lib", techniqueFileName),
      Path.Combine(Directory.GetCurrentDirectory(), "Techniques", techniqueFileName),
      Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lib", techniqueFileName),
      Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Techniques", techniqueFileName),
      Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EC-Lab Development Package", "lib", techniqueFileName),
      Path.Combine(AppContext.BaseDirectory, "lib", techniqueFileName)
    ];

    return candidatePaths.FirstOrDefault(File.Exists)
      ?? candidatePaths[0];
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
