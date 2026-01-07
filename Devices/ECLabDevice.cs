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
  public static readonly string DeviceName = "EC-LAB";

  private readonly ECLabCommunication communication;
  private readonly Dictionary<byte, ChannelInfo> channelInfos = new();
  private BOARD_TYPE boardType = BOARD_TYPE.ESSENTIAL;
  private readonly string? techniquesPath;

  public override string Name => DeviceName;

  public override bool IsBusy => false; // TODO: Implement based on channel states

  public bool IsConnected => this.communication.IsOpen;

  public int DeviceId => this.communication.DeviceId;

  public string TechniquesPath => this.GetTechniquesDirectory();

  public ECLabDevice(ECLabCommunication communication, string? techniquesPath = null)
  {
    this.communication = communication ?? throw new ArgumentNullException(nameof(communication));
    this.techniquesPath = techniquesPath;
    
    // Register communication with base Device class CommunicationController
    // This enables automatic connection management and OPC UA node integration
    this.communications.Add(new CommunicationController(this.Name, "VSP-3e", communication));
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
      bool success = this.Initialize();
      if (!success)
      {
        throw new InvalidOperationException("Failed to initialize ECLab device - firmware loading failed");
      }
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

      // Get library version
      try
      {
        string versionStr = ECLibApi.GetLibVersion();
        Log.Information("ECLib version: {VersionString}", versionStr);
      }
      catch (ECLibException ex)
      {
        Log.Warning("Failed to get ECLib version: {Error}", ex.Message);
      }

      // Determine board type BEFORE loading firmware
      // This ensures we load the correct firmware for the hardware
      try
      {
        int[] pluggedChannels = ECLibApi.GetChannelsPlugged(this.communication.DeviceId);
        if (pluggedChannels.Length > 0)
        {
          // Use first plugged channel to determine board type
          int firstChannel = pluggedChannels[0];
          this.boardType = ECLibApi.GetChannelBoardType(this.communication.DeviceId, firstChannel);
          Log.Information("Board type detected: {BoardType} (from channel {Channel})", this.boardType, firstChannel);
        }
        else
        {
          Log.Warning("No channels plugged, using default board type: {BoardType}", this.boardType);
        }
      }
      catch (ECLibException ex)
      {
        Log.Warning("Failed to determine board type: {Error}, using default: {BoardType}", ex.Message, this.boardType);
      }

      // Load firmware to all plugged channels AFTER determining board type
      // Note: BL_GetChannelInfos requires firmware to be loaded (returns -308 FIRM_FIRMWARENOTLOADED otherwise)
      Log.Information("Loading firmware to all plugged channels...");
      bool firmwareLoaded = this.LoadFirmwareAllChannels(force: false, showGauge: false);
      
      if (!firmwareLoaded)
      {
        Log.Error("Firmware loading failed - device initialization failed");
        return false;
      }

      // Now discover channels (firmware should be loaded)
      this.DiscoverChannels();

      Log.Information("Device initialized successfully");
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
            // Check if firmware not loaded error
            if (ex.ErrorCode == (int)BL_ERROR.FIRM_FIRMWARENOTLOADED)
            {
              Log.Warning("Channel {Channel}: Firmware not loaded. Please load firmware first.", ch);
            }
            else
            {
              Log.Warning("Failed to get info for channel {Channel}: {Error}", ch, ex.Message);
            }
          }
        }
      }

      // Board type已经在Initialize中确定，无需再次检测
    }
    catch (ECLibException ex)
    {
      Log.Warning("Failed to get plugged channels: {Error}", ex.Message);
    }
  }

  /// <summary>
  /// Determine board type for firmware selection
  /// Uses BL_GetChannelBoardType API to get actual hardware board type
  /// </summary>
  /// <param name="channel">Channel number (0-based)</param>
  /// <returns>Board type for firmware selection</returns>
  private BOARD_TYPE DetermineBoardType(byte channel)
  {
    try
    {
      // Use the actual BL_GetChannelBoardType API (channel is 0-based, API expects 1-based)
      BOARD_TYPE boardType = ECLibApi.GetChannelBoardType(this.communication.DeviceId, channel + 1);
      return boardType;
    }
    catch (Exception ex)
    {
      Log.Warning(ex, "Failed to get board type for channel {Channel}, defaulting to ESSENTIAL", channel);
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

      // Verify firmware files exist before loading
      if (!File.Exists(firmwarePath))
      {
        Log.Error("Firmware file not found: {FirmwarePath}", firmwarePath);
        return false;
      }

      if (!string.IsNullOrEmpty(fpgaPath) && !File.Exists(fpgaPath))
      {
        Log.Error("FPGA file not found: {FpgaPath}", fpgaPath);
        return false;
      }

      // Diagnostic: Log file attributes to verify accessibility
      try
      {
        FileInfo kernelInfo = new FileInfo(firmwarePath);
        Log.Debug("Kernel file: Path={Path}, Exists={Exists}, Length={Length}, ReadOnly={ReadOnly}", 
                  kernelInfo.FullName, kernelInfo.Exists, kernelInfo.Length, kernelInfo.IsReadOnly);
        
        if (!string.IsNullOrEmpty(fpgaPath))
        {
          FileInfo fpgaInfo = new FileInfo(fpgaPath);
          Log.Debug("FPGA file: Path={Path}, Exists={Exists}, Length={Length}, ReadOnly={ReadOnly}", 
                    fpgaInfo.FullName, fpgaInfo.Exists, fpgaInfo.Length, fpgaInfo.IsReadOnly);
        }
      }
      catch (Exception ex)
      {
        Log.Warning(ex, "Failed to read file attributes");
      }

      Log.Information(
          "Loading firmware for channel {Channel} (1-based: {OneBased}): Kernel={Firmware}, FPGA={Fpga}, BoardType={BoardType}, Force={Force}, ShowGauge={ShowGauge}",
          channel,
          channel + 1,
          firmwarePath,
          fpgaPath,
          this.boardType,
          force,
          showGauge);

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
          Log.Error("Firmware loading failed for channel {Channel}: {Error} (code: {Code})", 
                    channel + 1, errorMsg, result);
          
          // Special handling for XIL file not found error (-304)
          if (result == (int)BL_ERROR.FIRM_XILFILENOTEXISTS)
          {
            Log.Error("XIL (FPGA) file access failed. This may indicate:");
            Log.Error("  1. File permission issues with: {FpgaPath}", fpgaPath);
            Log.Error("  2. ECLib internal file access restrictions");
            Log.Error("  3. File is locked by another process");
            Log.Error("Suggestion: Try running the application as Administrator");
          }
          
          return false;
        }
      }
      catch (ECLibException ex)
      {
        Log.Error("Firmware loading failed: {Error}", ex.Message);
        
        // Special handling for communication errors that may indicate device state issues
        if (ex.ErrorCode == (int)BL_ERROR.COMM_ALLOCMEMFAILED || 
            ex.ErrorCode == (int)BL_ERROR.COMM_COMMFAILED)
        {
          Log.Error("Communication/memory error detected. Device may be in unstable state.");
          Log.Error("Suggestion: Disconnect and reconnect the device before retrying");
        }
        
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
  /// Returns relative paths based on TechniquesPath configuration from setting.json.
  /// Working directory is already set to setting.json location by App.xaml.cs.
  /// </summary>
  /// <param name="boardType">Board type to select appropriate firmware files</param>
  /// <param name="fpgaPath">Output: relative path to FPGA file</param>
  /// <returns>Relative path to kernel firmware file</returns>
  private string GetFirmwarePath(BOARD_TYPE boardType, out string fpgaPath)
  {
    // Use configured techniques path (relative to working directory)
    // Default to "lib" if not configured
    string firmwareDir = this.techniquesPath ?? "lib";

    Log.Debug("Using firmware directory: {FirmwareDir} (relative to working directory)", firmwareDir);

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
        return Path.Combine(firmwareDir, "kernel.bin");

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
  /// Get techniques directory path (relative to working directory)
  /// </summary>
  private string GetTechniquesDirectory()
  {
    return this.techniquesPath ?? "lib";
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
      "TechniquesPath" => this.TechniquesPath,
      _ => null
    };
  }
}
