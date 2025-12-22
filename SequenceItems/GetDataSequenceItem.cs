using Biologic.Native;
using DeviceControlSoftware;
using Serilog;
using System.Text;

namespace Biologic.SequenceItems;

/// <summary>
/// Get data from channel and save to file
/// Uses ECLibApi for type-safe data acquisition
/// </summary>
public class GetDataSequenceItem : SequenceItem
{
  private int _deviceId;
  private byte _channelIndex;
  private string _outputFilePath = string.Empty;
  private int _maxDataPoints = 10000;
  private bool _appendData = false;

  public override void Initialize(Sequence.StateContext context)
  {
    if (Properties == null)
      throw new ArgumentNullException(nameof(Properties));

    _channelIndex = Convert.ToByte(Properties.GetValueOrDefault("ChannelIndex") ?? 0);
    _outputFilePath = Properties.GetValueOrDefault("OutputFile")?.ToString() ?? "data.csv";
    _maxDataPoints = Convert.ToInt32(Properties.GetValueOrDefault("MaxDataPoints") ?? 10000);
    _appendData = Convert.ToBoolean(Properties.GetValueOrDefault("AppendData") ?? false);

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
  }

  public override Sequence.ResultTypes Execute(Sequence.StateContext context)
  {
    try
    {
      int totalPoints = 0;

      // Resolve output file path
      string fullPath = ResolveOutputPath(_outputFilePath);
      EnsureDirectoryExists(fullPath);

      using var fileStream = new FileStream(
          fullPath,
          _appendData ? FileMode.Append : FileMode.Create,
          FileAccess.Write,
          FileShare.Read);
      using var writer = new StreamWriter(fileStream, Encoding.UTF8);

      // Write header if not appending
      if (!_appendData || new FileInfo(fullPath).Length == 0)
      {
        writer.WriteLine("Time(s),Ewe(V),I(A),Cycle");
      }

      Log.Information("Reading data from channel {Channel} to {File}", _channelIndex, fullPath);

      // Get data using ECLibApi
      CurrentValues currentValues;
      DataInfo dataInfo;
      uint[] dataBuffer;

      try
      {
        (currentValues, dataInfo, dataBuffer) = ECLibApi.GetData(_deviceId, _channelIndex + 1); // Convert to 1-based
      }
      catch (ECLibException ex)
      {
        Log.Error("GetData failed: {ErrorMessage}", ex.Message);

        context.ResultParameter = new
        {
          Success = false,
          Message = $"Failed to get data: {ex.Message}"
        };

        return Sequence.ResultTypes.Error;
      }

      // Process data rows
      for (int row = 0; row < dataInfo.NbRows && totalPoints < _maxDataPoints; row++)
      {
        int offset = row * dataInfo.NbCols;

        if (offset + dataInfo.NbCols > dataBuffer.Length)
        {
          Log.Warning("Data buffer overflow at row {Row}", row);
          break;
        }

        // Parse data based on technique ID
        string dataLine = ParseDataRow(
            dataBuffer,
            offset,
            dataInfo.NbCols,
            dataInfo.TechniqueID,
            currentValues.ElapsedTime);

        writer.WriteLine(dataLine);
        totalPoints++;
      }

      Log.Information("Saved {Points} data points to {File}", totalPoints, fullPath);

      context.ResultParameter = new
      {
        Success = true,
        Message = $"Saved {totalPoints} data points",
        DataPoints = totalPoints,
        OutputFile = fullPath
      };

      return Sequence.ResultTypes.Next;
    }
    catch (Exception ex)
    {
      Log.Error(ex, "Unexpected error in GetData");

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

  private string ParseDataRow(uint[] dataBuffer, int offset, int cols, int techniqueID, double time)
  {
    // Parse based on technique type
    return techniqueID switch
    {
      100 => ParseOcvData(dataBuffer, offset, time), // OCV
      101 => ParseCpData(dataBuffer, offset, time),  // CP
      102 => ParseCaData(dataBuffer, offset, time),  // CA
      103 => ParseCvData(dataBuffer, offset, time),  // CV
      _ => ParseGenericData(dataBuffer, offset, cols, time)
    };
  }

  private string ParseOcvData(uint[] dataBuffer, int offset, double time)
  {
    // OCV: time, Ewe, [Ece]
    float ewe = ECLibApi.ConvertNumericIntoSingle(dataBuffer[offset + 2]);

    if (dataBuffer.Length > offset + 3)
    {
      float ece = ECLibApi.ConvertNumericIntoSingle(dataBuffer[offset + 3]);
      return $"{time:F6},{ewe:F6},,{ece:F6}";
    }

    return $"{time:F6},{ewe:F6},,";
  }

  private string ParseCpData(uint[] dataBuffer, int offset, double time)
  {
    // CP: time, Ewe, I, cycle
    float ewe = ECLibApi.ConvertNumericIntoSingle(dataBuffer[offset + 2]);
    float i = ECLibApi.ConvertNumericIntoSingle(dataBuffer[offset + 3]);
    uint cycle = dataBuffer[offset + 4];

    return $"{time:F6},{ewe:F6},{i:E6},{cycle}";
  }

  private string ParseCaData(uint[] dataBuffer, int offset, double time)
  {
    // CA: time, Ewe, I
    float ewe = ECLibApi.ConvertNumericIntoSingle(dataBuffer[offset + 2]);
    float i = ECLibApi.ConvertNumericIntoSingle(dataBuffer[offset + 3]);

    return $"{time:F6},{ewe:F6},{i:E6},";
  }

  private string ParseCvData(uint[] dataBuffer, int offset, double time)
  {
    // CV: time, Ewe, I, cycle
    float ewe = ECLibApi.ConvertNumericIntoSingle(dataBuffer[offset + 2]);
    float i = ECLibApi.ConvertNumericIntoSingle(dataBuffer[offset + 3]);
    uint cycle = dataBuffer[offset + 4];

    return $"{time:F6},{ewe:F6},{i:E6},{cycle}";
  }

  private string ParseGenericData(uint[] dataBuffer, int offset, int cols, double time)
  {
    var values = new List<string> { $"{time:F6}" };

    for (int i = 2; i < cols && offset + i < dataBuffer.Length; i++)
    {
      float value = ECLibApi.ConvertNumericIntoSingle(dataBuffer[offset + i]);
      values.Add($"{value:E6}");
    }

    return string.Join(",", values);
  }

  private string ResolveOutputPath(string path)
  {
    if (Path.IsPathRooted(path))
      return path;

    return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", path);
  }

  private void EnsureDirectoryExists(string filePath)
  {
    string? directory = Path.GetDirectoryName(filePath);
    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
    {
      Directory.CreateDirectory(directory);
    }
  }
}
