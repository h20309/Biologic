using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using Biologic.Communications;
using Biologic.Devices;
using Biologic.Native;
using DeviceControlSoftware;
using Serilog;

namespace Biologic;

/// <summary>
/// EC-Lab automation controller with real-time data polling capabilities
/// Extends ECLabSystem with automated data acquisition, monitoring, and OPC UA node updates.
/// This layer is client-facing and provides high-level automation APIs with OPC UA integration.
/// </summary>
public class ECLabAutomation : ECLabSystem, IDisposable
{
  private const int MaxEisHistoryCount = 3;
  private readonly ConcurrentDictionary<byte, ChannelDataPoller> channelPollers = new();
  private readonly ConcurrentDictionary<byte, ChannelEisHistory> eisHistoryByChannel = new();
  private readonly CancellationTokenSource cancellationTokenSource = new();
  private readonly IReadOnlyDictionary<string, Action<SequenceResult>> resultPublishers;
  private EventHandler<SequenceCompletedEventArgs>? sequenceCompletedHandler;
  private bool isPollingEnabled = false;
  private bool disposed = false;

  public ECLabAutomation()
  {
    this.resultPublishers = new Dictionary<string, Action<SequenceResult>>(StringComparer.OrdinalIgnoreCase)
    {
      ["RunGEIS"] = this.PublishGeisResult,
    };
  }

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

  protected override void Setup()
  {
    // Call base class setup first
    base.Setup();

    this.SubscribeResultPublishers();

    // Read polling configuration from settings
    bool enablePolling = bool.Parse(
      this.Settings?.Properties["EC-LAB"]?["EnableDataPolling"]?.ToString() ?? "false");

    if (enablePolling)
    {
      int pollingInterval = int.Parse(
        this.Settings?.Properties["EC-LAB"]?["PollingInterval"]?.ToString() ?? "1000");

      this.DefaultPollingInterval = pollingInterval;
      this.IsPollingEnabled = true;

      Log.Information("OPC UA data polling configured: Enabled={Enabled}, Interval={Interval}ms",
        enablePolling, pollingInterval);
    }
  }

  /// <summary>
  /// Start the automation system
  /// </summary>
  /// <remarks>
  /// Note: Polling is auto-started after device connection in ConnectDeviceSequenceItem,
  /// not here, because the device may not be connected yet when Start() is called.
  /// </remarks>
  public new void Start()
  {
    base.Start();
  }

  /// <summary>
  /// Stop the automation system and clean up polling
  /// </summary>
  public new void Stop()
  {
    StopAllPollers();
    base.Stop();
  }

  /// <summary>
  /// Start polling data for specific channel with OPC UA node updates
  /// </summary>
  /// <param name="channel">Channel index (0-based)</param>
  /// <param name="pollingIntervalMs">Polling interval in milliseconds</param>
  public void StartPolling(byte channel, int pollingIntervalMs = 0)
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

    // Create poller with OPC UA update callback
    var poller = new ChannelDataPoller(
        device,
        channel,
        pollingIntervalMs,
        UpdateOpcUaNodes, // Callback to update OPC UA nodes
        cancellationTokenSource.Token);

    if (channelPollers.TryAdd(channel, poller))
    {
      poller.Start();
      Log.Information("Started OPC UA polling for channel {Channel} with interval {Interval}ms",
        channel, pollingIntervalMs);
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
  /// Update OPC UA nodes with latest channel data
  /// This is called automatically by the data poller
  /// </summary>
  private void UpdateOpcUaNodes(ChannelDataBatch batch)
  {
    try
    {
      if (!TryGetChannelNode(batch.Channel, out Node? channelNode))
      {
        return;
      }

      // Update current values with safe node access
      // Use invariant culture to ensure consistent decimal separator (dot instead of comma)
      UpdateNodeValue(channelNode, "State", ((PROG_STATE)batch.CurrentValues.State).ToString());
      UpdateNodeValue(channelNode, "Voltage", batch.CurrentValues.Ewe.ToString(System.Globalization.CultureInfo.InvariantCulture));
      UpdateNodeValue(channelNode, "Current", batch.CurrentValues.I.ToString(System.Globalization.CultureInfo.InvariantCulture));
      UpdateNodeValue(channelNode, "ElapsedTime", batch.CurrentValues.ElapsedTime.ToString(System.Globalization.CultureInfo.InvariantCulture));
      UpdateNodeValue(channelNode, "Technique", batch.TechniqueId.ToString());

      // Update data statistics
      if (batch.DataInfo.NbRows > 0)
      {
        UpdateNodeValue(channelNode, "DataRows", batch.DataInfo.NbRows.ToString(System.Globalization.CultureInfo.InvariantCulture));
        UpdateNodeValue(channelNode, "DataCols", batch.DataInfo.NbCols.ToString(System.Globalization.CultureInfo.InvariantCulture));
        
        // Update LastUpdate as ISO 8601 DateTime string for OPC UA DateTime type
        UpdateNodeValue(channelNode, "LastUpdate", batch.Timestamp.ToUniversalTime().ToString("o"));

        Log.Debug("Updated OPC UA nodes for channel {Channel}: Rows={Rows}, State={State}, V={V:F6}V, I={I:F9}A",
          batch.Channel, batch.DataInfo.NbRows,
          (PROG_STATE)batch.CurrentValues.State,
          batch.CurrentValues.Ewe,
          batch.CurrentValues.I);
      }
    }
    catch (Exception ex)
    {
      Log.Error(ex, "Error updating OPC UA nodes for channel {Channel}", batch.Channel);
    }
  }

  private void SubscribeResultPublishers()
  {
    if (this.sequenceCompletedHandler != null)
    {
      return;
    }

    this.sequenceCompletedHandler = (_, args) => this.HandleSequenceCompleted(args.Result);
    this.SequenceController.SequenceCompleted += this.sequenceCompletedHandler;
  }

  private void HandleSequenceCompleted(SequenceResult result)
  {
    if (!result.Success || string.IsNullOrWhiteSpace(result.Result))
    {
      return;
    }

    if (!this.resultPublishers.TryGetValue(result.SequenceName, out Action<SequenceResult>? publisher))
    {
      return;
    }

    try
    {
      publisher(result);
    }
    catch (Exception ex)
    {
      Log.Error(ex, "Failed to publish sequence result for {SequenceName}", result.SequenceName);
    }
  }

  private void PublishGeisResult(SequenceResult result)
  {
    using JsonDocument document = JsonDocument.Parse(result.Result!);
    JsonElement root = document.RootElement.Clone();

    if (!TryGetChannelIndex(root, out byte channelIndex))
    {
      Log.Warning("Skipping GEIS result publication because ChannelIndex was missing. ContextId={ContextId}", result.ContextID);
      return;
    }

    int spectrumPointCount = GetSpectrumPointCount(root);
    var snapshot = new EisResultSnapshot
    {
      RunId = result.ContextID,
      SequenceName = result.SequenceName,
      ChannelIndex = channelIndex,
      CompletedAtUtc = result.EndTime.ToUniversalTime(),
      SpectrumPointCount = spectrumPointCount,
      Result = root,
    };

    ChannelEisHistory history = this.eisHistoryByChannel.GetOrAdd(channelIndex, static _ => new ChannelEisHistory());
    string historyJson;
    string latestResultJson = root.GetRawText();

    lock (history.SyncRoot)
    {
      history.Latest = snapshot;
      history.History.Insert(0, snapshot);
      while (history.History.Count > MaxEisHistoryCount)
      {
        history.History.RemoveAt(history.History.Count - 1);
      }

      historyJson = SequenceDispatcher.Serialize(history.History);
    }

    this.UpdateLatestEisNodes(channelIndex, snapshot, latestResultJson, historyJson);
  }

  private void UpdateLatestEisNodes(byte channelIndex, EisResultSnapshot snapshot, string latestResultJson, string historyJson)
  {
    if (!TryGetChannelNode(channelIndex, out Node? channelNode))
    {
      return;
    }

    UpdateNodeValue(channelNode, "LatestEISStatus", "Available");
    UpdateNodeValue(channelNode, "LatestEISTimestamp", snapshot.CompletedAtUtc.ToString("o", CultureInfo.InvariantCulture));
    UpdateNodeValue(channelNode, "LatestEISResultJson", latestResultJson);
    UpdateNodeValue(channelNode, "LatestEISHistoryJson", historyJson);
    UpdateNodeValue(channelNode, "LatestEISPointCount", snapshot.SpectrumPointCount.ToString(CultureInfo.InvariantCulture));
    UpdateNodeValue(channelNode, "LatestEISRunId", snapshot.RunId.ToString(CultureInfo.InvariantCulture));

    Log.Information(
      "Published {SequenceName} result for channel {Channel} to OPC UA history cache. RunId={RunId}, Points={PointCount}",
      snapshot.SequenceName,
      channelIndex,
      snapshot.RunId,
      snapshot.SpectrumPointCount);
  }

  private bool TryGetChannelNode(byte channel, out Node? channelNode)
  {
    channelNode = null;

    var device = this.devices.Values.FirstOrDefault() as ECLabDevice;
    if (device?.Node == null)
    {
      Log.Warning("Cannot access OPC UA channel nodes: device node not found");
      return false;
    }

    var channelsFolder = device.Node["Channels"];
    if (channelsFolder == null || string.IsNullOrEmpty(channelsFolder.Name))
    {
      Log.Warning("Channels folder node not found in OPC UA address space");
      return false;
    }

    string channelName = $"Channel{channel}";
    channelNode = channelsFolder[channelName];
    if (channelNode == null || string.IsNullOrEmpty(channelNode.Name))
    {
      Log.Warning("Channel node not found: {ChannelName}. Only explicitly modeled channels can expose long-lived EIS data.", channelName);
      return false;
    }

    return true;
  }

  private static bool TryGetChannelIndex(JsonElement root, out byte channelIndex)
  {
    channelIndex = 0;

    if (!root.TryGetProperty("ChannelIndex", out JsonElement channelElement))
    {
      return false;
    }

    if (!channelElement.TryGetInt32(out int numericChannel) || numericChannel < byte.MinValue || numericChannel > byte.MaxValue)
    {
      return false;
    }

    channelIndex = (byte)numericChannel;
    return true;
  }

  private static int GetSpectrumPointCount(JsonElement root)
  {
    if (root.TryGetProperty("SpectrumPointCount", out JsonElement pointCountElement) &&
        pointCountElement.TryGetInt32(out int pointCount))
    {
      return pointCount;
    }

    if (root.TryGetProperty("Spectrum", out JsonElement spectrumElement) && spectrumElement.ValueKind == JsonValueKind.Array)
    {
      int count = 0;
      foreach (JsonElement _ in spectrumElement.EnumerateArray())
      {
        count++;
      }

      return count;
    }

    return 0;
  }

  /// <summary>
  /// Safely update a child node value with error handling
  /// </summary>
  private void UpdateNodeValue(Node parentNode, string childName, string value)
  {
    try
    {
      var childNode = parentNode[childName];
      if (childNode != null && !string.IsNullOrEmpty(childNode.Name))
      {
        Log.Debug("Writing to node {NodeName}: '{Value}'", childName, value);
        childNode.Value = value;
        Log.Debug("Successfully wrote to node {NodeName}", childName);
      }
      else
      {
        Log.Warning("Child node '{ChildName}' not found under '{ParentName}'", childName, parentNode.Name);
      }
    }
    catch (Exception ex)
    {
      Log.Warning(ex, "Failed to update node '{ChildName}' with value '{Value}'", childName, value);
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
        if (this.sequenceCompletedHandler != null)
        {
          this.SequenceController.SequenceCompleted -= this.sequenceCompletedHandler;
          this.sequenceCompletedHandler = null;
        }

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

internal sealed class ChannelEisHistory
{
  public object SyncRoot { get; } = new();

  public EisResultSnapshot? Latest { get; set; }

  public List<EisResultSnapshot> History { get; } = new();
}

internal sealed class EisResultSnapshot
{
  public uint RunId { get; init; }

  public string SequenceName { get; init; } = string.Empty;

  public byte ChannelIndex { get; init; }

  public DateTime CompletedAtUtc { get; init; }

  public int SpectrumPointCount { get; init; }

  public JsonElement Result { get; init; }
}
