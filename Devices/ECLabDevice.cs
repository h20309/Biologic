using Biologic;
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
  private const int FirmwareLoadRetryCount = 3;
  private const int FirmwareLoadRetryDelayMs = 1500;
  private const bool ForceFirmwareReloadOnOpen = true;

  private readonly ECLabCommunication communication;
  private readonly Dictionary<byte, ChannelInfo> channelInfos = new();
  private readonly ECLabPathResolver pathResolver;
  private BOARD_TYPE boardType = BOARD_TYPE.ESSENTIAL;
  private readonly string? techniquesPath;
  private readonly string? eclibDirectory;
  private bool isInitialized;

  public override string Name => DeviceName;

  public override bool IsBusy => this.communication.IsBusyConnection;

  public new bool IsConnected => this.communication.IsOpen;

  public bool IsInitialized => this.isInitialized;

  public bool HasAvailableChannels => this.channelInfos.Count > 0;

  public int DeviceId => this.communication.DeviceId;

  public string TechniquesPath => this.GetTechniquesDirectory();

  public string ECLibDirectory => this.GetECLibDirectory();

  public ECLabDevice(
    ECLabCommunication communication,
    string? techniquesPath = null,
    string? eclibDirectory = null,
    string? configurationRoot = null)
  {
    this.communication = communication ?? throw new ArgumentNullException(nameof(communication));
    this.techniquesPath = techniquesPath;
    this.eclibDirectory = eclibDirectory;
    this.pathResolver = new ECLabPathResolver(configurationRoot);
    
    // Register communication with base Device class CommunicationController
    // This enables automatic connection management and OPC UA node integration
    this.communications.Add(new CommunicationController(this.Name, "VSP-3e", communication));
  }

  /// <summary>
  /// Open device - called by Device framework
  /// </summary>
  public override void Open()
  {
    if (!this.communication.IsOpen)
    {
      Log.Information("Opening ECLab communication before initialization");
      if (!this.communication.Open())
      {
        string message = this.communication.LastOpenErrorMessage ?? "Failed to open ECLab communication";
        throw new InvalidOperationException(message);
      }
    }

    if (this.communication.IsOpen)
    {
      if (this.isInitialized && this.channelInfos.Count > 0)
      {
        Log.Information(
          "ECLab device {DeviceId} is already initialized with {ChannelCount} channel(s); skipping duplicate firmware load",
          this.communication.DeviceId,
          this.channelInfos.Count);
        return;
      }

      bool success = this.Initialize(forceFirmwareReload: ForceFirmwareReloadOnOpen);
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
    Log.Information("Closing ECLabDevice");
    this.isInitialized = false;
    this.channelInfos.Clear();
    this.communication.Close();
  }

  /// <summary>
  /// Initialize device - discover channels and get version
  /// </summary>
  public bool Initialize(bool forceFirmwareReload = true)
  {
    try
    {
      if (!this.communication.IsOpen)
      {
        Log.Warning("Cannot initialize ECLabDevice: communication not open");
        return false;
      }

      if (this.isInitialized && this.channelInfos.Count > 0)
      {
        Log.Information(
          "ECLab device {DeviceId} already initialized; skipping repeated initialization",
          this.communication.DeviceId);
        return true;
      }

      Log.Information("Initializing ECLab device {DeviceId}", this.communication.DeviceId);
      Log.Information("  Configuration root: {Root}", this.pathResolver.ConfigurationRoot);
      Log.Information("  Techniques directory: {Path}", this.TechniquesPath);
      Log.Information("  Native library directory: {Path}", this.ECLibDirectory);
      Log.Information("  Firmware load mode: ForceReload={ForceFirmwareReload}", forceFirmwareReload);

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
      Log.Information("Loading firmware to all plugged channels with force={ForceFirmwareReload}...", forceFirmwareReload);
      bool firmwareLoaded = this.LoadFirmwareAllChannels(force: forceFirmwareReload, showGauge: false);
      
      if (!firmwareLoaded)
      {
        Log.Error("Firmware loading failed - device initialization failed");
        return false;
      }

      // Now discover channels (firmware should be loaded)
      this.DiscoverChannels();

      this.isInitialized = this.channelInfos.Count > 0;

      Log.Information("Device initialized successfully");
      return this.isInitialized;
    }
    catch (Exception ex)
    {
      this.isInitialized = false;
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

      // Board type is already determined in Initialize, so no re-detection is needed.
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
      BOARD_TYPE channelBoardType = this.GetBoardTypeForChannel(channel);
      string firmwarePath = this.GetFirmwarePath(channelBoardType, out string fpgaPath);

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

      string nativeFirmwarePath = this.pathResolver.ToNativePath(firmwarePath);
      string nativeFpgaPath = string.IsNullOrEmpty(fpgaPath)
        ? string.Empty
        : this.pathResolver.ToNativePath(fpgaPath);
      var loadVariants = this.BuildFirmwareLoadPathVariants(firmwarePath, fpgaPath, nativeFirmwarePath, nativeFpgaPath);

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
          "Loading firmware for channel {Channel} (1-based: {OneBased}): Kernel={Firmware}, FPGA={Fpga}, NativeKernel={NativeFirmware}, NativeFpga={NativeFpga}, BoardType={BoardType}, Force={Force}, ShowGauge={ShowGauge}",
          channel,
          channel + 1,
          firmwarePath,
          fpgaPath,
          nativeFirmwarePath,
          nativeFpgaPath,
          channelBoardType,
          force,
          showGauge);

      int? lastResultCode = null;
      ECLibException? lastEclibException = null;

      foreach (var variant in loadVariants)
      {
        Log.Information(
          "Trying firmware load path variant {VariantName}: WorkingDirectory={WorkingDirectory}, KernelArg={KernelArg}, FpgaArg={FpgaArg}",
          variant.Name,
          variant.WorkingDirectory,
          variant.FirmwareArgument,
          string.IsNullOrEmpty(variant.FpgaArgument) ? "(none)" : variant.FpgaArgument);

        for (int attempt = 1; attempt <= FirmwareLoadRetryCount; attempt++)
        {
          try
          {
            Dictionary<int, int> results;
            using (PushWorkingDirectory(variant.WorkingDirectory))
            {
              results = ECLibApi.LoadFirmware(
                this.communication.DeviceId,
                new[] { channel + 1 },
                showGauge,
                force,
                variant.FirmwareArgument,
                variant.FpgaArgument);
            }

            if (!results.TryGetValue(channel + 1, out int result) || result == 0)
            {
              goto FirmwareLoaded;
            }

            lastResultCode = result;
            string errorMsg = ECLibApi.GetErrorMessage(result);
            bool isRetriable = this.IsRetriableFirmwareError(result);

            if (isRetriable && attempt < FirmwareLoadRetryCount)
            {
              Log.Warning(
                "Firmware loading attempt {Attempt}/{MaxAttempts} failed for channel {Channel} using variant {VariantName}: {Error} (code: {Code}). Retrying after {DelayMs} ms.",
                attempt,
                FirmwareLoadRetryCount,
                channel + 1,
                variant.Name,
                errorMsg,
                result,
                FirmwareLoadRetryDelayMs);
              Thread.Sleep(FirmwareLoadRetryDelayMs);
              continue;
            }

            Log.Warning(
              "Firmware loading variant {VariantName} failed for channel {Channel}: {Error} (code: {Code})",
              variant.Name,
              channel + 1,
              errorMsg,
              result);

            if (result == (int)BL_ERROR.FIRM_XILFILENOTEXISTS)
            {
              Log.Warning("XIL (FPGA) file resolution failed for variant {VariantName}", variant.Name);
            }

            if (IsFirmwarePathResolutionError(result))
            {
              break;
            }

            return false;
          }
          catch (ECLibException ex)
          {
            lastEclibException = ex;
            bool isRetriable = this.IsRetriableFirmwareError(ex.ErrorCode);
            if (isRetriable && attempt < FirmwareLoadRetryCount)
            {
              Log.Warning(
                "Firmware loading attempt {Attempt}/{MaxAttempts} raised retriable error for channel {Channel} using variant {VariantName}: {Error} (code: {Code}). Retrying after {DelayMs} ms.",
                attempt,
                FirmwareLoadRetryCount,
                channel + 1,
                variant.Name,
                ex.Message,
                ex.ErrorCode,
                FirmwareLoadRetryDelayMs);
              Thread.Sleep(FirmwareLoadRetryDelayMs);
              continue;
            }

            Log.Warning(
              "Firmware loading variant {VariantName} raised error for channel {Channel}: {Error}",
              variant.Name,
              channel + 1,
              ex.Message);

            if (IsFirmwarePathResolutionError(ex.ErrorCode))
            {
              break;
            }

            if (ex.ErrorCode == (int)BL_ERROR.COMM_ALLOCMEMFAILED ||
                ex.ErrorCode == (int)BL_ERROR.COMM_COMMFAILED)
            {
              Log.Error("Communication/memory error detected. Device may be in unstable state.");
              Log.Error("Suggestion: Disconnect and reconnect the device before retrying");
            }

            return false;
          }
        }
      }

      if (lastEclibException != null)
      {
        Log.Error("Firmware loading failed after exhausting all path variants: {Error}", lastEclibException.Message);
      }
      else if (lastResultCode.HasValue)
      {
        Log.Error(
          "Firmware loading failed after exhausting all path variants: {Error} (code: {Code})",
          ECLibApi.GetErrorMessage(lastResultCode.Value),
          lastResultCode.Value);
      }

      if (lastResultCode == (int)BL_ERROR.FIRM_XILFILENOTEXISTS ||
          lastEclibException?.ErrorCode == (int)BL_ERROR.FIRM_XILFILENOTEXISTS)
      {
        Log.Error("XIL (FPGA) file access failed across all path variants. This strongly suggests an ECLib path-resolution issue rather than a transient connection failure.");
      }

      return false;

FirmwareLoaded:

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
  /// Force a clean channel state by stopping execution and reloading firmware.
  /// This is used when technique loading leaves residual protocol state on the channel.
  /// </summary>
  public bool ResetChannelTechniqueState(byte channel, bool forceFirmwareReload = true)
  {
    if (!this.communication.IsOpen)
    {
      Log.Error("Cannot reset channel {Channel}: device not connected", channel);
      return false;
    }

    Log.Information(
      "Resetting channel {Channel} technique state. ForceFirmwareReload={ForceFirmwareReload}",
      channel,
      forceFirmwareReload);

    try
    {
      try
      {
        ECLibApi.StopChannel(this.communication.DeviceId, channel + 1);
      }
      catch (ECLibException ex)
      {
        Log.Warning("StopChannel during reset for channel {Channel} reported: {Error}", channel, ex.Message);
      }

      bool loaded = this.LoadFirmware(channel, force: forceFirmwareReload, showGauge: false);
      if (!loaded)
      {
        Log.Error("Channel {Channel} reset failed because firmware reload did not succeed", channel);
        return false;
      }

      try
      {
        var info = ECLibApi.GetChannelInfo(this.communication.DeviceId, channel + 1);
        this.channelInfos[channel] = info;
      }
      catch (ECLibException ex)
      {
        Log.Warning("Channel {Channel} reset completed but channel info refresh failed: {Error}", channel, ex.Message);
      }

      return true;
    }
    catch (Exception ex)
    {
      Log.Error(ex, "Unexpected error while resetting channel {Channel} technique state", channel);
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

      Log.Information("Loading firmware for {Count} channel(s) with force={Force}, showGauge={ShowGauge}", pluggedChannels.Length, force, showGauge);

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
    string firmwareDir = this.TechniquesPath;

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

  public string ResolveTechniqueFilePath(string techniqueFile, byte? channel = null)
  {
    if (string.IsNullOrWhiteSpace(techniqueFile))
    {
      throw new ArgumentException("Technique file name must be provided", nameof(techniqueFile));
    }

    string resolvedFileName = this.ResolveTechniqueFileName(techniqueFile, channel);
    return Path.Combine(this.TechniquesPath, resolvedFileName);
  }

  public string GetNativeTechniqueFilePath(string techniqueFile, byte? channel = null)
  {
    return this.pathResolver.ToNativePath(this.ResolveTechniqueFilePath(techniqueFile, channel));
  }

  /// <summary>
  /// Get techniques directory path (relative to working directory)
  /// </summary>
  private string GetTechniquesDirectory()
  {
    return this.pathResolver.ResolveDirectory(this.techniquesPath, "lib");
  }

  private string GetECLibDirectory()
  {
    return this.pathResolver.ResolveDirectory(this.eclibDirectory ?? this.techniquesPath, "lib");
  }

  private bool IsRetriableFirmwareError(int errorCode)
  {
    return errorCode == (int)BL_ERROR.FIRM_XILFILENOTEXISTS ||
           errorCode == (int)BL_ERROR.COMM_COMMFAILED ||
           errorCode == (int)BL_ERROR.COMM_ALLOCMEMFAILED;
  }

  private static bool IsFirmwarePathResolutionError(int errorCode)
  {
    return errorCode == (int)BL_ERROR.FIRM_FIRMFILENOTEXISTS ||
           errorCode == (int)BL_ERROR.FIRM_FIRMFILEACCESSFAILED ||
           errorCode == (int)BL_ERROR.FIRM_XILFILENOTEXISTS ||
           errorCode == (int)BL_ERROR.FIRM_XILFILEACCESSFAILED;
  }

  private IReadOnlyList<FirmwareLoadPathVariant> BuildFirmwareLoadPathVariants(
    string firmwarePath,
    string fpgaPath,
    string nativeFirmwarePath,
    string nativeFpgaPath)
  {
    var variants = new List<FirmwareLoadPathVariant>();
    string techniquesDirectory = this.TechniquesPath;
    string configurationRoot = this.pathResolver.ConfigurationRoot;
    string firmwareFileName = Path.GetFileName(firmwarePath);
    string fpgaFileName = string.IsNullOrEmpty(fpgaPath) ? string.Empty : Path.GetFileName(fpgaPath);

    variants.Add(new FirmwareLoadPathVariant(
      "oem-filename-from-lib",
      techniquesDirectory,
      firmwareFileName,
      fpgaFileName));

    variants.Add(new FirmwareLoadPathVariant(
      "relative-from-config-root",
      configurationRoot,
      Path.GetRelativePath(configurationRoot, firmwarePath),
      string.IsNullOrEmpty(fpgaPath) ? string.Empty : Path.GetRelativePath(configurationRoot, fpgaPath)));

    variants.Add(new FirmwareLoadPathVariant(
      "absolute-native-path",
      configurationRoot,
      nativeFirmwarePath,
      nativeFpgaPath));

    return variants;
  }

  private static IDisposable PushWorkingDirectory(string workingDirectory)
  {
    return new WorkingDirectoryScope(workingDirectory);
  }

  private BOARD_TYPE GetBoardTypeForChannel(byte channel)
  {
    try
    {
      return this.DetermineBoardType(channel);
    }
    catch
    {
      return this.boardType;
    }
  }

  private string ResolveTechniqueFileName(string techniqueFile, byte? channel)
  {
    string extension = Path.GetExtension(techniqueFile);
    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(techniqueFile);

    if (channel is null ||
        fileNameWithoutExtension.EndsWith("4", StringComparison.OrdinalIgnoreCase) ||
        fileNameWithoutExtension.EndsWith("5", StringComparison.OrdinalIgnoreCase))
    {
      return techniqueFile;
    }

    string candidateFileName = this.GetBoardTypeForChannel(channel.Value) switch
    {
      BOARD_TYPE.PREMIUM => $"{fileNameWithoutExtension}4{extension}",
      BOARD_TYPE.DIGICORE => $"{fileNameWithoutExtension}5{extension}",
      _ => techniqueFile
    };

    string candidatePath = Path.Combine(this.TechniquesPath, candidateFileName);
    return File.Exists(candidatePath) ? candidateFileName : techniqueFile;
  }

  /// <summary>
  /// Get property by name for SequenceItem compatibility
  /// </summary>
  public new object? GetProperty(string propertyName)
  {
    return propertyName switch
    {
      "DeviceId" => this.communication.DeviceId,
      "IsOpen" => this.communication.IsOpen,
      "IsError" => this.communication.IsError,
      "DeviceInfo" => this.communication.DeviceInfo,
      "ChannelCount" => this.channelInfos.Count,
      "IsInitialized" => this.IsInitialized,
      "TechniquesPath" => this.TechniquesPath,
      "ECLibDirectory" => this.ECLibDirectory,
      "BoardType" => this.boardType,
      _ => null
    };
  }

  private sealed record FirmwareLoadPathVariant(
    string Name,
    string WorkingDirectory,
    string FirmwareArgument,
    string FpgaArgument);

  private sealed class WorkingDirectoryScope : IDisposable
  {
    private readonly string previousDirectory;

    public WorkingDirectoryScope(string workingDirectory)
    {
      this.previousDirectory = Directory.GetCurrentDirectory();
      Directory.SetCurrentDirectory(workingDirectory);
    }

    public void Dispose()
    {
      Directory.SetCurrentDirectory(this.previousDirectory);
    }
  }
}
