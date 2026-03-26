using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
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
  private const int MaxChargePointWindow = 1000;
  private readonly ConcurrentDictionary<byte, ChannelDataPoller> channelPollers = new();
  private readonly ConcurrentDictionary<byte, ChannelEisHistory> eisHistoryByChannel = new();
  private readonly ConcurrentDictionary<uint, byte> directlyPublishedGeisRunIds = new();
  private readonly ConcurrentDictionary<byte, ChargeRunState> chargeRunsByChannel = new();
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
  /// Polling is not auto-started here.
  /// Use ConnectDeviceSequenceItem to start polling after an explicit successful connection.
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
  /// Ensure polling is active for all currently connected channels.
  /// Safe to call repeatedly.
  /// </summary>
  public void EnsurePollingForConnectedChannels()
  {
    if (!this.IsPollingEnabled)
    {
      return;
    }

    var device = this.devices.Values.FirstOrDefault() as ECLabDevice;
    if (device == null)
    {
      Log.Warning("Cannot ensure polling: device not found");
      return;
    }

    if (!device.IsConnected || !device.HasAvailableChannels)
    {
      Log.Information("Skipping polling startup because device is not connected or has no available channels yet");
      return;
    }

    try
    {
      int[] channels = ECLibApi.GetChannelsPlugged(device.DeviceId);
      foreach (int ch in channels)
      {
        byte channel = (byte)(ch - 1);
        this.StartPolling(channel);
      }

      Log.Information("Ensured data polling for {Count} connected channel(s)", channels.Length);
    }
    catch (Exception ex)
    {
      Log.Warning(ex, "Failed to ensure polling for connected channels");
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

  public void BeginChargeRun(byte channel, float targetVoltage, float durationSeconds, float recordIntervalSeconds, string? outputFilePath)
  {
    string resolvedOutputPath = ResolveChargeOutputPath(channel, outputFilePath);
    var state = new ChargeRunState
    {
      RunId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
      ChannelIndex = channel,
      TargetVoltage_V = targetVoltage,
      Duration_s = durationSeconds,
      RecordInterval_s = recordIntervalSeconds,
      StartedAtUtc = DateTime.UtcNow,
      UpdatedAtUtc = DateTime.UtcNow,
      Status = "Running",
      CsvPath = resolvedOutputPath
    };

    this.chargeRunsByChannel[channel] = state;
    this.UpdateLatestChargeNodes(channel, state, "[]");

    Log.Information(
      "Initialized Charge run tracking for channel {Channel}. Target={Voltage}V, Duration={Duration}s, Csv={CsvPath}",
      channel,
      targetVoltage,
      durationSeconds,
      resolvedOutputPath);
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
      if (batch.DataInfo.NbRows > 0)
      {
        UpdateNodeValue(channelNode, "Technique", batch.TechniqueId.ToString());
      }
      else if ((PROG_STATE)batch.CurrentValues.State == PROG_STATE.STOP)
      {
        UpdateNodeValue(channelNode, "Technique", "Idle");
      }

      UpdateNodeValue(channelNode, "DataRows", batch.DataInfo.NbRows.ToString(System.Globalization.CultureInfo.InvariantCulture));
      UpdateNodeValue(channelNode, "DataCols", batch.DataInfo.NbCols.ToString(System.Globalization.CultureInfo.InvariantCulture));
      UpdateNodeValue(channelNode, "LastUpdate", batch.Timestamp.ToUniversalTime().ToString("o"));

      // Update data statistics
      if (batch.DataInfo.NbRows > 0)
      {
        Log.Debug("Updated OPC UA nodes for channel {Channel}: Rows={Rows}, State={State}, V={V:F6}V, I={I:F9}A",
          batch.Channel, batch.DataInfo.NbRows,
          (PROG_STATE)batch.CurrentValues.State,
          batch.CurrentValues.Ewe,
          batch.CurrentValues.I);
      }

      this.UpdateChargeRun(batch);
    }
    catch (Exception ex)
    {
      Log.Error(ex, "Error updating OPC UA nodes for channel {Channel}", batch.Channel);
    }
  }

  private void UpdateChargeRun(ChannelDataBatch batch)
  {
    if (!this.chargeRunsByChannel.TryGetValue(batch.Channel, out ChargeRunState? state))
    {
      return;
    }

    string seriesJson;
    bool publishCompletedSnapshot;
    lock (state.SyncRoot)
    {
      if (batch.DataInfo.NbRows > 0)
      {
        IReadOnlyList<ChargeTimeSeriesPoint> newPoints = ParseChargePoints(batch);
        if (newPoints.Count > 0)
        {
          AppendChargePoints(state, newPoints);
        }
      }

      state.UpdatedAtUtc = batch.Timestamp.ToUniversalTime();
      state.Status = DetermineChargeStatus(state, batch.CurrentValues.State);
      seriesJson = SequenceDispatcher.Serialize(state.Points);
      publishCompletedSnapshot = state.Status == "Completed" && !state.FinalSnapshotPublished;
    }

    if (publishCompletedSnapshot)
    {
      seriesJson = PublishCompletedChargeSnapshot(state);
    }

    this.UpdateLatestChargeNodes(batch.Channel, state, seriesJson);
  }

  private static string PublishCompletedChargeSnapshot(ChargeRunState state)
  {
    List<ChargeTimeSeriesPoint> finalPoints = LoadChargePointsFromCsv(state);

    lock (state.SyncRoot)
    {
      state.FinalSnapshotPublished = true;
      if (finalPoints.Count > 0)
      {
        state.TotalPointCount = Math.Max(state.TotalPointCount, finalPoints.Count);
      }
    }

    Log.Information(
      "Published final Charge snapshot for channel {Channel} from CSV. Csv={CsvPath}, Points={PointCount}",
      state.ChannelIndex,
      state.CsvPath,
      finalPoints.Count);

    return SequenceDispatcher.Serialize(finalPoints.Count > 0 ? finalPoints : state.Points);
  }

  private static List<ChargeTimeSeriesPoint> LoadChargePointsFromCsv(ChargeRunState state)
  {
    var points = new List<ChargeTimeSeriesPoint>();
    if (string.IsNullOrWhiteSpace(state.CsvPath) || !File.Exists(state.CsvPath))
    {
      return points;
    }

    foreach (string line in File.ReadLines(state.CsvPath))
    {
      if (string.IsNullOrWhiteSpace(line) || line.StartsWith("Time(s)", StringComparison.OrdinalIgnoreCase))
      {
        continue;
      }

      string[] parts = line.Split(',', StringSplitOptions.TrimEntries);
      if (parts.Length < 4)
      {
        continue;
      }

      if (!double.TryParse(parts[0], NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out double timeSeconds) ||
          !float.TryParse(parts[1], NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out float ewe) ||
          !float.TryParse(parts[2], NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out float current) ||
          !uint.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out uint cycle))
      {
        continue;
      }

      points.Add(new ChargeTimeSeriesPoint
      {
        ChannelIndex = state.ChannelIndex,
        CapturedAtUtc = state.UpdatedAtUtc,
        Time_s = timeSeconds,
        Ewe_V = ewe,
        I_A = current,
        Cycle = cycle
      });
    }

    return points;
  }

  private void UpdateLatestChargeNodes(byte channelIndex, ChargeRunState state, string seriesJson)
  {
    if (!TryGetChannelNode(channelIndex, out Node? channelNode))
    {
      return;
    }

    UpdateNodeValue(channelNode, "LatestChargeStatus", state.Status);
    UpdateNodeValue(channelNode, "LatestChargeUpdatedAt", state.UpdatedAtUtc.ToString("o", CultureInfo.InvariantCulture));
    UpdateNodeValue(channelNode, "LatestChargeSeriesJson", seriesJson);
    UpdateNodeValue(channelNode, "LatestChargePointCount", state.TotalPointCount.ToString(CultureInfo.InvariantCulture));
    UpdateNodeValue(channelNode, "LatestChargeCsvPath", state.CsvPath);
  }

  private static IReadOnlyList<ChargeTimeSeriesPoint> ParseChargePoints(ChannelDataBatch batch)
  {
    if (batch.BoardType == 0 || batch.DataInfo.NbRows <= 0 || batch.DataInfo.NbCols < 5)
    {
      return Array.Empty<ChargeTimeSeriesPoint>();
    }

    if (batch.TechniqueId != TechniqueId.CA && batch.TechniqueId != TechniqueId.CALIMIT)
    {
      return Array.Empty<ChargeTimeSeriesPoint>();
    }

    int totalWords = Math.Min(batch.DataBuffer.Length, batch.DataInfo.NbRows * batch.DataInfo.NbCols);
    var points = new List<ChargeTimeSeriesPoint>(batch.DataInfo.NbRows);

    for (int rowIndex = 0; rowIndex < batch.DataInfo.NbRows; rowIndex++)
    {
      int offset = rowIndex * batch.DataInfo.NbCols;
      if (offset + 4 >= totalWords)
      {
        break;
      }

      double timeSeconds = ECLibApi.ConvertTimeChannelNumericIntoSeconds(
        batch.DataBuffer[offset],
        batch.DataBuffer[offset + 1],
        batch.CurrentValues.TimeBase,
        batch.BoardType);

      float ewe = ECLibApi.ConvertChannelNumericIntoSingle(batch.DataBuffer[offset + 2], batch.BoardType);
      float current = ECLibApi.ConvertChannelNumericIntoSingle(batch.DataBuffer[offset + 3], batch.BoardType);
      uint cycle = batch.DataBuffer[offset + 4];

      points.Add(new ChargeTimeSeriesPoint
      {
        ChannelIndex = batch.Channel,
        CapturedAtUtc = batch.Timestamp.ToUniversalTime(),
        Time_s = timeSeconds,
        Ewe_V = ewe,
        I_A = current,
        Cycle = cycle
      });
    }

    return points;
  }

  private static void AppendChargePoints(ChargeRunState state, IReadOnlyList<ChargeTimeSeriesPoint> newPoints)
  {
    if (newPoints.Count == 0)
    {
      return;
    }

    EnsureChargeCsvInitialized(state);

    using var writer = new StreamWriter(new FileStream(state.CsvPath, FileMode.Append, FileAccess.Write, FileShare.Read));
    foreach (ChargeTimeSeriesPoint point in newPoints)
    {
      string pointKey = point.GetStableKey();
      if (!state.SeenPointKeys.Add(pointKey))
      {
        continue;
      }

      state.Points.Add(point);
      while (state.Points.Count > MaxChargePointWindow)
      {
        state.Points.RemoveAt(0);
      }

      state.TotalPointCount++;
      writer.WriteLine(point.ToCsvRow());
    }
  }

  private static void EnsureChargeCsvInitialized(ChargeRunState state)
  {
    if (state.CsvInitialized)
    {
      return;
    }

    string? directory = Path.GetDirectoryName(state.CsvPath);
    if (!string.IsNullOrWhiteSpace(directory))
    {
      Directory.CreateDirectory(directory);
    }

    bool shouldWriteHeader = !File.Exists(state.CsvPath) || new FileInfo(state.CsvPath).Length == 0;
    if (shouldWriteHeader)
    {
      using var writer = new StreamWriter(new FileStream(state.CsvPath, FileMode.Create, FileAccess.Write, FileShare.Read));
      writer.WriteLine("Time(s),Ewe(V),I(A),Cycle");
    }

    state.CsvInitialized = true;
  }

  private static string DetermineChargeStatus(ChargeRunState state, int channelState)
  {
    if (state.TotalPointCount == 0 && (PROG_STATE)channelState == PROG_STATE.STOP)
    {
      return "WaitingForData";
    }

    return (PROG_STATE)channelState switch
    {
      PROG_STATE.RUN => "Running",
      PROG_STATE.PAUSE => "Paused",
      PROG_STATE.STOP when state.TotalPointCount > 0 => "Completed",
      _ => state.Status
    };
  }

  private static string ResolveChargeOutputPath(byte channel, string? outputFilePath)
  {
    if (!string.IsNullOrWhiteSpace(outputFilePath))
    {
      return Path.IsPathRooted(outputFilePath)
        ? outputFilePath
        : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", outputFilePath);
    }

    string fileName = $"charge-channel{channel}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";
    return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", fileName);
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

    if (string.Equals(result.SequenceName, "RunGEIS", StringComparison.OrdinalIgnoreCase) &&
        this.directlyPublishedGeisRunIds.TryRemove(result.ContextID, out _))
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

  public void PublishGeisResultPayload(uint runId, string sequenceName, DateTime completedAtUtc, string resultJson)
  {
    this.PublishGeisResultCore(runId, sequenceName, completedAtUtc, resultJson);
    this.directlyPublishedGeisRunIds[runId] = 0;
  }

  private void PublishGeisResult(SequenceResult result)
  {
    this.PublishGeisResultCore(result.ContextID, result.SequenceName, result.EndTime.ToUniversalTime(), result.Result!);
  }

  private void PublishGeisResultCore(uint runId, string sequenceName, DateTime completedAtUtc, string resultJson)
  {
    using JsonDocument document = JsonDocument.Parse(resultJson);
    JsonElement root = document.RootElement.Clone();

    if (!TryGetChannelIndex(root, out byte channelIndex))
    {
      Log.Warning("Skipping GEIS result publication because ChannelIndex was missing. ContextId={ContextId}", runId);
      return;
    }

    int spectrumPointCount = GetSpectrumPointCount(root);
    var snapshot = new EisResultSnapshot
    {
      RunId = runId,
      SequenceName = sequenceName,
      ChannelIndex = channelIndex,
      CompletedAtUtc = completedAtUtc,
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

  private bool TryGetChannelNode(byte channel, [NotNullWhen(true)] out Node? channelNode)
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
  private readonly uint boardType;
  private readonly int pollingIntervalMs;
  private readonly Action<ChannelDataBatch>? dataHandler;
  private readonly CancellationToken externalToken;
  private readonly CancellationTokenSource pollerCts = new();
  private readonly ConcurrentQueue<ChannelDataBatch> dataCache = new();
  private readonly int maxCacheSize = 100;

  private CancellationTokenSource? linkedCts;
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
    this.boardType = ResolveBoardType(device, channel);
    this.pollingIntervalMs = pollingIntervalMs;
    this.dataHandler = dataHandler;
    this.externalToken = cancellationToken;
  }

  public void Start()
  {
    if (pollingTask != null && !pollingTask.IsCompleted)
    {
      return;
    }

    linkedCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken, pollerCts.Token);
    pollingTask = Task.Run(async () => await PollLoopAsync(linkedCts.Token), linkedCts.Token);
  }

  public void Stop()
  {
    pollerCts.Cancel();
    try
    {
      pollingTask?.Wait(TimeSpan.FromSeconds(5));
    }
    catch (AggregateException)
    {
      // Expected when task is cancelled
    }
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

  private async Task PollLoopAsync(CancellationToken cancellationToken)
  {
    Log.Debug("Starting poll loop for channel {Channel}", channel);

    while (!cancellationToken.IsCancellationRequested)
    {
      try
      {
        if (!device.GetCurrentValues(channel, out CurrentValues currentValues))
        {
          await Task.Delay(pollingIntervalMs, cancellationToken);
          continue;
        }

        var batch = new ChannelDataBatch
        {
          Timestamp = DateTime.Now,
          Channel = channel,
          BoardType = boardType,
          CurrentValues = currentValues,
          DataInfo = default,
          DataBuffer = Array.Empty<uint>(),
          TechniqueId = latestBatch?.TechniqueId ?? default
        };

        if ((PROG_STATE)currentValues.State == PROG_STATE.RUN)
        {
          try
          {
            var (dataCurrentValues, dataInfo, dataBuffer) = ECLibApi.GetData(device.DeviceId, channel + 1);
            batch.CurrentValues = dataCurrentValues;
            batch.DataInfo = dataInfo;
            batch.DataBuffer = dataBuffer;
            if (dataInfo.NbRows > 0)
            {
              batch.TechniqueId = (TechniqueId)dataInfo.TechniqueID;
            }
          }
          catch (ECLibException ex)
          {
            Log.Warning(ex, "Error getting data for channel {Channel}; publishing live state only for this poll cycle", channel);
          }
        }

        latestBatch = batch;

        if (batch.DataInfo.NbRows > 0)
        {
          dataCache.Enqueue(batch);
          while (dataCache.Count > maxCacheSize)
          {
            dataCache.TryDequeue(out _);
          }
        }

        dataHandler?.Invoke(batch);

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

  private static uint ResolveBoardType(ECLabDevice device, byte channel)
  {
    try
    {
      return (uint)ECLibApi.GetChannelBoardType(device.DeviceId, channel + 1);
    }
    catch (Exception ex)
    {
      Log.Warning(ex, "Failed to resolve board type for channel {Channel}. Charge data conversion will be skipped until this succeeds.", channel);
      return 0;
    }
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

  /// <summary>Board type used by BL_ConvertChannelNumericIntoSingle</summary>
  public uint BoardType { get; set; }

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
        result[row] = BoardType != 0
          ? ECLibApi.ConvertChannelNumericIntoSingle(DataBuffer[offset], BoardType)
          : ECLibApi.ConvertNumericIntoSingle(DataBuffer[offset]);
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
      if (offset + 1 < DataBuffer.Length && BoardType != 0)
      {
        result[row] = ECLibApi.ConvertTimeChannelNumericIntoSeconds(
          DataBuffer[offset],
          DataBuffer[offset + 1],
          CurrentValues.TimeBase,
          BoardType);
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

internal sealed class ChargeRunState
{
  public object SyncRoot { get; } = new();

  public long RunId { get; init; }

  public byte ChannelIndex { get; init; }

  public float TargetVoltage_V { get; init; }

  public float Duration_s { get; init; }

  public float RecordInterval_s { get; init; }

  public DateTime StartedAtUtc { get; init; }

  public DateTime UpdatedAtUtc { get; set; }

  public string Status { get; set; } = "Idle";

  public string CsvPath { get; init; } = string.Empty;

  public bool CsvInitialized { get; set; }

  public bool FinalSnapshotPublished { get; set; }

  public int TotalPointCount { get; set; }

  public List<ChargeTimeSeriesPoint> Points { get; } = new();

  public HashSet<string> SeenPointKeys { get; } = new(StringComparer.Ordinal);
}

internal sealed class ChargeTimeSeriesPoint
{
  public byte ChannelIndex { get; init; }

  public DateTime CapturedAtUtc { get; init; }

  public double Time_s { get; init; }

  public float Ewe_V { get; init; }

  public float I_A { get; init; }

  public uint Cycle { get; init; }

  public string GetStableKey()
  {
    return string.Create(
      CultureInfo.InvariantCulture,
      $"{ChannelIndex}:{Cycle}:{Time_s:F9}:{Ewe_V:F9}:{I_A:F9}");
  }

  public string ToCsvRow()
  {
    return string.Create(
      CultureInfo.InvariantCulture,
      $"{Time_s:F9},{Ewe_V:F9},{I_A:F9},{Cycle}");
  }
}
