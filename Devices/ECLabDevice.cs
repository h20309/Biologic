using Biologic.Communications;
using Biologic.Native;
using DeviceControlSoftware;
using Serilog;

namespace Biologic.Devices;

/// <summary>
/// BioLogic ECLab device
/// Encapsulates channel management, firmware loading, and data acquisition
/// </summary>
public class ECLabDevice : Device
{
  public const string DeviceName = "ECLabDevice";

  private readonly ECLabCommunication communication;
  private readonly Dictionary<byte, ChannelInfo> channelInfos = new();
  private BOARD_TYPE boardType = BOARD_TYPE.ESSENTIAL;

  public override string Name => DeviceName;

  public override bool IsBusy => false; // TODO: Implement based on channel states

  public ECLabDevice(ECLabCommunication communication)
  {
    this.communication = communication ?? throw new ArgumentNullException(nameof(communication));
  }

  /// <summary>
  /// Open device - called by Device framework
  /// </summary>
  public override void Open()
  {
    // Communication is managed by CommunicationController
    // Just initialize the device after communication is open
    if (this.communication.IsOpen)
    {
      this.Initialize();
    }
  }

  /// <summary>
  /// Close device - called by Device framework
  /// </summary>
  public override void Close()
  {
    // Communication closing is managed by CommunicationController
    // Clean up any device-specific resources here if needed
    Log.Information("Closing ECLabDevice");
  }

  /// <summary>
  /// Initialize device - discover channels and get version
  /// </summary>
  public bool Initialize()
  {
    try
    {
      if (!this.communication.IsOpen)
      {
        Log.Warning("Cannot initialize ECLabDevice: communication not open");
        return false;
      }

      Log.Information("Initializing ECLab device {DeviceId}", this.communication.DeviceId);

      // Discover channels
      this.DiscoverChannels();

      // Get library version
      try
      {
        var (version, versionStr) = ECLibApi.GetLibVersion();
        Log.Information("ECLib version: {Version} ({VersionString})", version, versionStr);
      }
      catch (ECLibException ex)
      {
        Log.Warning("Failed to get ECLib version: {Error}", ex.Message);
      }

      return true;
    }
    catch (Exception ex)
    {
      Log.Error(ex, "Error initializing ECLab device");
      return false;
    }
  }

  /// <summary>
  /// Discover and enumerate plugged channels
  /// </summary>
  private void DiscoverChannels()
  {
    if (!this.communication.IsOpen)
    {
      Log.Warning("Cannot discover channels: communication not open");
      return;
    }

    try
    {
      int[] pluggedChannels = ECLibApi.GetChannelsPlugged(this.communication.DeviceId);

      // Convert 1-based array back to 0-based byte array for internal storage
      byte[] channelsPlugged = new byte[16];
      foreach (int ch in pluggedChannels)
      {
        if (ch >= 1 && ch <= 16)
        {
          channelsPlugged[ch - 1] = 1;
        }
      }

      this.channelInfos.Clear();

      for (byte ch = 0; ch < 16; ch++)
      {
        if (channelsPlugged[ch] != 0)
        {
          try
          {
            ChannelInfo info = ECLibApi.GetChannelInfo(this.communication.DeviceId, ch + 1); // Convert to 1-based
            this.channelInfos[ch] = info;
            Log.Information(
                "Channel {Channel} detected - Board: {Board}, Firmware: {Firmware}, State: {State}",
                ch,
                info.BoardVersion,
                info.FirmwareVersion,
                (PROG_STATE)info.State);
          }
          catch (ECLibException ex)
          {
            Log.Warning("Failed to get info for channel {Channel}: {Error}", ch, ex.Message);
          }
        }
      }

      // Determine board type from first channel
      if (this.channelInfos.Count > 0)
      {
        byte firstChannel = this.channelInfos.Keys.First();
        this.boardType = this.DetermineBoardType(firstChannel);
        Log.Information("Board type detected: {BoardType}", this.boardType);
      }
    }
    catch (ECLibException ex)
    {
      Log.Warning("Failed to get plugged channels: {Error}", ex.Message);
    }
  }

  /// <summary>
  /// Determine board type for firmware selection
  /// </summary>
  private BOARD_TYPE DetermineBoardType(byte channel)
  {
    try
    {
      var deviceCode = (DEVICE)this.communication.DeviceInfo.DeviceCode;

      return deviceCode switch
      {
        DEVICE.VMP3 or DEVICE.VMP3E => BOARD_TYPE.ESSENTIAL,
        DEVICE.VSP or DEVICE.VSP300 or DEVICE.VSP3E => BOARD_TYPE.PREMIUM,
        DEVICE.SP50 or DEVICE.SP100 or DEVICE.SP150 or
        DEVICE.SP200 or DEVICE.SP240 or DEVICE.SP300 => BOARD_TYPE.DIGICORE,
        _ => BOARD_TYPE.ESSENTIAL
      };
    }
    catch
    {
      return BOARD_TYPE.ESSENTIAL;
    }
  }

  /// <summary>
  /// Load firmware to specified channel
  /// </summary>
  public bool LoadFirmware(byte channel, bool force = false, bool showGauge = false)
  {
    if (!this.communication.IsOpen)
    {
      Log.Error("Cannot load firmware: device not connected");
      return false;
    }

    try
    {
      string firmwarePath = this.GetFirmwarePath(this.boardType, out string fpgaPath);

      Log.Information(
          "Loading firmware for channel {Channel}: {Firmware}, FPGA: {Fpga}",
          channel,
          firmwarePath,
          fpgaPath);

      try
      {
        var results = ECLibApi.LoadFirmware(
            this.communication.DeviceId,
            new[] { channel + 1 }, // Convert to 1-based
            showGauge,
            force,
            firmwarePath,
            fpgaPath);

        // Check if this channel had an error
        if (results.TryGetValue(channel + 1, out int result) && result != 0)
        {
          string errorMsg = ECLibApi.GetErrorMessage(result);
          Log.Error("Firmware loading failed: {Error}", errorMsg);
          return false;
        }
      }
      catch (ECLibException ex)
      {
        Log.Error("Firmware loading failed: {Error}", ex.Message);
        return false;
      }

      // Refresh channel info after firmware loading
      try
      {
        var info = ECLibApi.GetChannelInfo(this.communication.DeviceId, channel + 1); // Convert to 1-based
        this.channelInfos[channel] = info;
      }
      catch (ECLibException ex)
      {
        Log.Warning("Failed to refresh channel info: {Error}", ex.Message);
      }

      Log.Information("Firmware loaded successfully for channel {Channel}", channel);
      return true;
    }
    catch (Exception ex)
    {
      Log.Error(ex, "Error loading firmware");
      return false;
    }
  }

  /// <summary>
  /// Load firmware to all plugged channels
  /// </summary>
  /// <param name="force">Force firmware reload</param>
  /// <param name="showGauge">Show progress gauge</param>
  /// <returns>True if all channels loaded successfully</returns>
  /// <remarks>
  /// This is a convenience method that loads firmware to all plugged channels.
  /// Note: This operation can take 20-40 seconds as it loads kernel + FPGA firmware.
  /// </remarks>
  public bool LoadFirmwareAllChannels(bool force = false, bool showGauge = false)
  {
    if (!this.communication.IsOpen)
    {
      Log.Error("Cannot load firmware: device not connected");
      return false;
    }

    try
    {
      // Get all plugged channels
      int[] pluggedChannels = ECLibApi.GetChannelsPlugged(this.communication.DeviceId);
      
      if (pluggedChannels.Length == 0)
      {
        Log.Warning("No channels plugged");
        return false;
      }

      Log.Information("Loading firmware for {Count} channel(s)", pluggedChannels.Length);

      // Load firmware for each channel
      bool allSuccess = true;
      foreach (int ch in pluggedChannels)
      {
        byte channel = (byte)(ch - 1); // Convert to 0-based
        bool success = LoadFirmware(channel, force, showGauge);
        if (!success)
        {
          Log.Error("Failed to load firmware for channel {Channel}", channel);
          allSuccess = false;
        }
      }

      if (allSuccess)
      {
        Log.Information("? Firmware loaded successfully for all channels");
      }
      else
      {
        Log.Warning("? Some channels failed to load firmware");
      }

      return allSuccess;
    }
    catch (Exception ex)
    {
      Log.Error(ex, "Error loading firmware for all channels");
      return false;
    }
  }

  /// <summary>
  /// Get firmware file paths based on board type
  /// Files are loaded from the deployed lib/ directory
  /// </summary>
  private string GetFirmwarePath(BOARD_TYPE boardType, out string fpgaPath)
  {
    // Use deployed EC-Lab Development Package directory
    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
    string firmwareDir = Path.Combine(baseDir, "EC-Lab Development Package", "lib");

    // Fallback to simple lib/ directory if EC-Lab package not found
    if (!Directory.Exists(firmwareDir))
    {
      firmwareDir = Path.Combine(baseDir, "lib");
      
      // Final fallback to system installation
      if (!Directory.Exists(firmwareDir))
      {
        Log.Warning("lib directory not found in application directory, using system installation");
        firmwareDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            "EC-Lab Development Package",
            "Data");
      }
    }

    Log.Debug("Using firmware directory: {FirmwareDir}", firmwareDir);

    switch (boardType)
    {
      case BOARD_TYPE.ESSENTIAL:
        fpgaPath = Path.Combine(firmwareDir, "vmp_ii_0437_a6.xlx");
        return Path.Combine(firmwareDir, "kernel.bin");

      case BOARD_TYPE.PREMIUM:
        fpgaPath = Path.Combine(firmwareDir, "Vmp_iv_0395_aa.xlx");
        return Path.Combine(firmwareDir, "kernel4.bin");

      case BOARD_TYPE.DIGICORE:
        fpgaPath = string.Empty;
        return Path.Combine(firmwareDir, "kernel5.bin");

      default:
        throw new NotSupportedException($"Unsupported board type: {boardType}");
    }
  }

  /// <summary>
  /// Get current values for a channel
  /// </summary>
  public bool GetCurrentValues(byte channel, out CurrentValues values)
  {
    if (!this.communication.IsOpen)
    {
      values = default;
      return false;
    }

    try
    {
      values = ECLibApi.GetCurrentValues(this.communication.DeviceId, channel + 1); // Convert to 1-based
      return true;
    }
    catch (ECLibException)
    {
      values = default;
      return false;
    }
  }

  /// <summary>
  /// Test device connection
  /// </summary>
  public bool TestConnection()
  {
    return this.communication.TestConnection();
  }

  /// <summary>
  /// Get property by name for SequenceItem compatibility
  /// </summary>
  public object? GetProperty(string propertyName)
  {
    return propertyName switch
    {
      "DeviceId" => this.communication.DeviceId,
      "IsOpen" => this.communication.IsOpen,
      "IsError" => this.communication.IsError,
      "DeviceInfo" => this.communication.DeviceInfo,
      "ChannelCount" => this.channelInfos.Count,
      _ => null
    };
  }
}
