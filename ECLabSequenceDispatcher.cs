using System.Collections.Concurrent;
using Biologic.Communications;
using Biologic.Devices;
using Biologic.Native;
using DeviceControlSoftware;
using Serilog;

namespace Biologic;

/// <summary>
/// Advanced sequence dispatcher with data polling capabilities
/// Extends BiologicSystem with real-time data acquisition and monitoring
/// </summary>
public class ECLabSequenceDispatcher : BiologicSystem, IDisposable
{
  private readonly ConcurrentDictionary<byte, ChannelDataPoller> channelPollers = new();
  private readonly CancellationTokenSource cancellationTokenSource = new();
  private bool isPollingEnabled = false;
  private bool disposed = false;

  /// <summary>
  /// Enable or disable automatic data polling
  /// </summary>
  public bool IsPollingEnabled
  {
    get => isPollingEnabled;
    set
    {
      if (value != isPollingEnabled)
      {
        isPollingEnabled = value;
        if (value)
        {
          Log.Information("Data polling enabled");
        }
        else
        {
          Log.Information("Data polling disabled");
          StopAllPollers();
        }
      }
    }
  }

  /// <summary>
  /// Default polling interval in milliseconds
  /// </summary>
  public int DefaultPollingInterval { get; set; } = 1000;

  /// <summary>
  /// Start polling data for specific channel
  /// </summary>
  /// <param name="channel">Channel index (0-based)</param>
  /// <param name="pollingIntervalMs">Polling interval in milliseconds</param>
  /// <param name="dataHandler">Optional callback for each data batch</param>
  public void StartPolling(byte channel, int pollingIntervalMs = 0, Action<ChannelDataBatch>? dataHandler = null)
  {
    if (pollingIntervalMs <= 0)
      pollingIntervalMs = DefaultPollingInterval;

    var device = this.devices.Values.FirstOrDefault() as ECLabDevice;
    if (device == null)
    {
      Log.Error("Cannot start polling: device not found");
      return;
    }

    if (channelPollers.TryGetValue(channel, out var existingPoller))
    {
      Log.Warning("Polling already started for channel {Channel}", channel);
      return;
    }

    var poller = new ChannelDataPoller(
        device,
        channel,
        pollingIntervalMs,
        dataHandler,
        cancellationTokenSource.Token);

    if (channelPollers.TryAdd(channel, poller))
    {
      poller.Start();
      Log.Information("Started polling for channel {Channel} with interval {Interval}ms", channel, pollingIntervalMs);
    }
  }

  /// <summary>
  /// Stop polling data for specific channel
  /// </summary>
  public void StopPolling(byte channel)
  {
    if (channelPollers.TryRemove(channel, out var poller))
    {
      poller.Stop();
      Log.Information("Stopped polling for channel {Channel}", channel);
    }
  }

  /// <summary>
  /// Stop all active pollers
  /// </summary>
  public void StopAllPollers()
  {
    foreach (var kvp in channelPollers)
    {
      kvp.Value.Stop();
    }
    channelPollers.Clear();
    Log.Information("Stopped all channel pollers");
  }

  /// <summary>
  /// Get latest data batch for channel
  /// </summary>
  public ChannelDataBatch? GetLatestData(byte channel)
  {
    if (channelPollers.TryGetValue(channel, out var poller))
    {
      return poller.GetLatestBatch();
    }
    return null;
  }

  /// <summary>
  /// Get all cached data batches for channel
  /// </summary>
  public List<ChannelDataBatch> GetAllData(byte channel)
  {
    if (channelPollers.TryGetValue(channel, out var poller))
    {
      return poller.GetAllBatches();
    }
    return new List<ChannelDataBatch>();
  }

  /// <summary>
  /// Clear cached data for channel
  /// </summary>
  public void ClearData(byte channel)
  {
    if (channelPollers.TryGetValue(channel, out var poller))
    {
      poller.ClearCache();
      Log.Information("Cleared data cache for channel {Channel}", channel);
    }
  }

  /// <summary>
  /// Cleanup on disposal
  /// </summary>
  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  /// <summary>
  /// Protected dispose pattern
  /// </summary>
  protected virtual void Dispose(bool disposing)
  {
    if (!disposed)
    {
      if (disposing)
      {
        cancellationTokenSource.Cancel();
        StopAllPollers();
        cancellationTokenSource.Dispose();
      }
      disposed = true;
    }
  }
}

/// <summary>
/// Channel data poller - handles background data acquisition for a single channel
/// </summary>
internal class ChannelDataPoller
{
  private readonly ECLabDevice device;
  private readonly byte channel;
  private readonly int pollingIntervalMs;
  private readonly Action<ChannelDataBatch>? dataHandler;
  private readonly CancellationToken cancellationToken;
  private readonly ConcurrentQueue<ChannelDataBatch> dataCache = new();
  private readonly int maxCacheSize = 100;

  private Task? pollingTask;
  private ChannelDataBatch? latestBatch;

  public ChannelDataPoller(
      ECLabDevice device,
      byte channel,
      int pollingIntervalMs,
      Action<ChannelDataBatch>? dataHandler,
      CancellationToken cancellationToken)
  {
    this.device = device;
    this.channel = channel;
    this.pollingIntervalMs = pollingIntervalMs;
    this.dataHandler = dataHandler;
    this.cancellationToken = cancellationToken;
  }

  public void Start()
  {
    if (pollingTask != null && !pollingTask.IsCompleted)
    {
      return;
    }

    pollingTask = Task.Run(async () => await PollLoopAsync(), cancellationToken);
  }

  public void Stop()
  {
    // Polling will stop when cancellationToken is triggered
  }

  public ChannelDataBatch? GetLatestBatch()
  {
    return latestBatch;
  }

  public List<ChannelDataBatch> GetAllBatches()
  {
    return dataCache.ToList();
  }

  public void ClearCache()
  {
    while (dataCache.TryDequeue(out _)) { }
  }

  private async Task PollLoopAsync()
  {
    Log.Debug("Starting poll loop for channel {Channel}", channel);

    while (!cancellationToken.IsCancellationRequested)
    {
      try
      {
        // Get current values to check if channel is running
        if (!device.GetCurrentValues(channel, out CurrentValues currentValues))
        {
          await Task.Delay(pollingIntervalMs, cancellationToken);
          continue;
        }

        // Only get data if channel is running
        if (currentValues.State != (int)PROG_STATE.RUN)
        {
          await Task.Delay(pollingIntervalMs, cancellationToken);
          continue;
        }

        // Get data batch
        try
        {
          var (values, dataInfo, dataBuffer) = ECLibApi.GetData(
              device.GetProperty("DeviceId") as int? ?? 0,
              channel + 1); // Convert to 1-based

          if (dataInfo.NbRows > 0)
          {
            var batch = new ChannelDataBatch
            {
              Timestamp = DateTime.Now,
              Channel = channel,
              CurrentValues = values,
              DataInfo = dataInfo,
              DataBuffer = dataBuffer,
              TechniqueId = (TechniqueId)dataInfo.TechniqueID
            };

            // Update latest batch
            latestBatch = batch;

            // Add to cache
            dataCache.Enqueue(batch);

            // Trim cache if too large
            while (dataCache.Count > maxCacheSize)
            {
              dataCache.TryDequeue(out _);
            }

            // Call handler if provided
            dataHandler?.Invoke(batch);

            Log.Debug(
                "Polled {Rows} data points from channel {Channel}, technique {Technique}",
                dataInfo.NbRows,
                channel,
                (TechniqueId)dataInfo.TechniqueID);
          }
        }
        catch (ECLibException ex)
        {
          Log.Warning("Error getting data for channel {Channel}: {Error}", channel, ex.Message);
        }

        // Wait for next poll
        await Task.Delay(pollingIntervalMs, cancellationToken);
      }
      catch (OperationCanceledException)
      {
        // Normal cancellation
        break;
      }
      catch (Exception ex)
      {
        Log.Error(ex, "Unexpected error in poll loop for channel {Channel}", channel);
        await Task.Delay(pollingIntervalMs, cancellationToken);
      }
    }

    Log.Debug("Poll loop stopped for channel {Channel}", channel);
  }
}

/// <summary>
/// Container for a batch of channel data
/// </summary>
public class ChannelDataBatch
{
  /// <summary>Timestamp when data was acquired</summary>
  public DateTime Timestamp { get; set; }

  /// <summary>Channel index (0-based)</summary>
  public byte Channel { get; set; }

  /// <summary>Current values at time of acquisition</summary>
  public CurrentValues CurrentValues { get; set; }

  /// <summary>Data information</summary>
  public DataInfo DataInfo { get; set; }

  /// <summary>Raw data buffer</summary>
  public uint[] DataBuffer { get; set; } = Array.Empty<uint>();

  /// <summary>Technique that generated this data</summary>
  public TechniqueId TechniqueId { get; set; }

  /// <summary>
  /// Convert raw data to float array for specific column
  /// </summary>
  public float[] GetColumnAsFloats(int columnIndex)
  {
    if (columnIndex < 0 || columnIndex >= DataInfo.NbCols)
      throw new ArgumentOutOfRangeException(nameof(columnIndex));

    var result = new float[DataInfo.NbRows];
    for (int row = 0; row < DataInfo.NbRows; row++)
    {
      int offset = row * DataInfo.NbCols + columnIndex;
      if (offset < DataBuffer.Length)
      {
        result[row] = ECLibApi.ConvertNumericIntoSingle(DataBuffer[offset]);
      }
    }
    return result;
  }

  /// <summary>
  /// Get time column (usually column 0)
  /// </summary>
  public double[] GetTimeColumn()
  {
    var result = new double[DataInfo.NbRows];
    for (int row = 0; row < DataInfo.NbRows; row++)
    {
      int offset = row * DataInfo.NbCols;
      if (offset < DataBuffer.Length)
      {
        // Time is typically in columns 0-1 (high and low parts)
        result[row] = CurrentValues.ElapsedTime + (row * CurrentValues.TimeBase);
      }
    }
    return result;
  }

  /// <summary>
  /// Get all data as CSV string
  /// </summary>
  public string ToCsv()
  {
    var sb = new System.Text.StringBuilder();

    // Header
    sb.AppendLine($"# Channel: {Channel}");
    sb.AppendLine($"# Technique: {TechniqueId}");
    sb.AppendLine($"# Timestamp: {Timestamp:yyyy-MM-dd HH:mm:ss}");
    sb.AppendLine($"# Rows: {DataInfo.NbRows}, Cols: {DataInfo.NbCols}");
    sb.AppendLine();

    // Data
    for (int row = 0; row < DataInfo.NbRows; row++)
    {
      var values = new List<string>();
      for (int col = 0; col < DataInfo.NbCols; col++)
      {
        int offset = row * DataInfo.NbCols + col;
        if (offset < DataBuffer.Length)
        {
          float value = ECLibApi.ConvertNumericIntoSingle(DataBuffer[offset]);
          values.Add(value.ToString("G"));
        }
      }
      sb.AppendLine(string.Join(",", values));
    }

    return sb.ToString();
  }
}
