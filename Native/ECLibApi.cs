using System.Text;
using Serilog;

namespace Biologic.Native;

/// <summary>
/// High-level, type-safe API for EClib64.dll
/// </summary>
/// <remarks>
/// This class provides a user-friendly wrapper around the low-level P/Invoke declarations
/// in <see cref="ECLibNative"/>. It handles error checking, resource management, and provides
/// a more C#-idiomatic interface similar to Python's KBIO_api.
/// </remarks>
public class ECLibApi
{
  #region Connection Management

  /// <summary>
  /// Connect to a BioLogic device
  /// </summary>
  /// <param name="address">Device address (e.g., "USB0", "192.168.1.100")</param>
  /// <param name="timeout">Connection timeout in seconds (default: 5s)</param>
  /// <returns>Tuple containing device ID and device information</returns>
  /// <exception cref="ECLibException">Thrown when connection fails</exception>
  /// <remarks>
  /// Note: After connection, you need to call LoadFirmware() to load kernel and FPGA firmware
  /// before using the channels. This is a separate explicit step.
  /// </remarks>
  public static (int DeviceId, DeviceInfo Info) Connect(string address, byte timeout = 5)
  {
    int errorCode = ECLibNative.BL_Connect(address, timeout, out int deviceId, out DeviceInfo info);
    CheckError(errorCode, $"Connect to {address}");
    return (deviceId, info);
  }

  /// <summary>
  /// Connect to device and automatically load firmware for all plugged channels
  /// </summary>
  /// <param name="address">Device address</param>
  /// <param name="timeout">Connection timeout in seconds</param>
  /// <param name="showProgress">Show firmware loading progress</param>
  /// <param name="forceFirmware">Force firmware reload</param>
  /// <returns>Tuple containing device ID and device information</returns>
  /// <remarks>
  /// This is a convenience method that combines Connect + LoadFirmware.
  /// Use this when you want to quickly connect and load firmware in one call.
  /// For more control, use Connect() followed by LoadFirmware() separately.
  /// </remarks>
  public static (int DeviceId, DeviceInfo Info) ConnectAndLoadFirmware(
    string address, 
    byte timeout = 5,
    bool showProgress = true,
    bool forceFirmware = false)
  {
    // Step 1: Connect
    var (deviceId, info) = Connect(address, timeout);
    
    // Step 2: Get plugged channels
    int[] channels = GetChannelsPlugged(deviceId);
    
    if (channels.Length == 0)
    {
      throw new ECLibException(-3, "ConnectAndLoadFirmware", "No channels plugged");
    }
    
    // Step 3: Load firmware for all channels
    // Note: firmware paths will be determined automatically based on board type
    var results = LoadFirmware(
      deviceId, 
      channels, 
      showGauge: showProgress,
      force: forceFirmware,
      firmwarePath: null,  // Use default
      fpgaPath: null);     // Use default
    
    // Check if any channel failed
    foreach (var kvp in results)
    {
      if (kvp.Value != 0)
      {
        string errorMsg = GetErrorMessage(kvp.Value);
        throw new ECLibException(
          kvp.Value, 
          $"ConnectAndLoadFirmware - Channel {kvp.Key}", 
          errorMsg);
      }
    }
    
    return (deviceId, info);
  }

  /// <summary>
  /// Disconnect from a BioLogic device
  /// </summary>
  /// <param name="deviceId">Device identifier obtained from <see cref="Connect"/></param>
  /// <exception cref="ECLibException">Thrown when disconnection fails</exception>
  public static void Disconnect(int deviceId)
  {
    int errorCode = ECLibNative.BL_Disconnect(deviceId);
    CheckError(errorCode, $"Disconnect device {deviceId}");
  }

  /// <summary>
  /// Test connection to a device
  /// </summary>
  /// <param name="deviceId">Device identifier</param>
  /// <returns>True if connection is valid</returns>
  public static bool TestConnection(int deviceId)
  {
    int errorCode = ECLibNative.BL_TestConnection(deviceId);
    return errorCode == 0;
  }

  #endregion

  #region Device Information

  /// <summary>
  /// Get library version information
  /// </summary>
  /// <returns>Version string</returns>
  public static string GetLibVersion()
  {
    try
    {
      var versionStr = new StringBuilder(256);
      uint size = (uint)versionStr.Capacity;
      int errorCode = ECLibNative.BL_GetLibVersion(versionStr, ref size);
      
      CheckError(errorCode, "GetLibVersion");
      
      return versionStr.ToString().TrimEnd('\0');
    }
    catch (DllNotFoundException ex)
    {
      throw new ECLibException(-1, "GetLibVersion", 
        $"EClib64.dll not found. Please install EC-Lab Development Package. Details: {ex.Message}");
    }
    catch (AccessViolationException ex)
    {
      throw new ECLibException(-2, "GetLibVersion", 
        $"Access violation when calling EClib64.dll. The DLL may be incompatible or corrupted. Details: {ex.Message}");
    }
    catch (Exception ex)
    {
      throw new ECLibException(-3, "GetLibVersion", 
        $"Unexpected error calling EClib64.dll: {ex.Message}");
    }
  }

  public static string? GetLoadedLibraryPath()
  {
    return ECLibNative.GetLoadedModulePath("EClib64.dll");
  }

  /// <summary>
  /// Get channel information
  /// </summary>
  /// <param name="deviceId">Device identifier</param>
  /// <param name="channel">Channel number (1-based: 1..16)</param>
  /// <returns>Channel information structure</returns>
  public static ChannelInfo GetChannelInfo(int deviceId, int channel)
  {
    byte channelByte = (byte)(channel - 1); // Convert to 0-based
    int errorCode = ECLibNative.BL_GetChannelInfos(deviceId, channelByte, out ChannelInfo info);
    CheckError(errorCode, $"GetChannelInfo for device {deviceId}, channel {channel}");
    return info;
  }

  /// <summary>
  /// Get which channels are plugged
  /// </summary>
  /// <param name="deviceId">Device identifier</param>
  /// <returns>Array of channel indices that are plugged (1-based)</returns>
  public static int[] GetChannelsPlugged(int deviceId)
  {
    byte[] channels = new byte[16];
    int errorCode = ECLibNative.BL_GetChannelsPlugged(deviceId, channels, (byte)channels.Length);
    CheckError(errorCode, $"GetChannelsPlugged for device {deviceId}");

    // Convert to 1-based channel list
    var pluggedChannels = new List<int>();
    for (int i = 0; i < channels.Length; i++)
    {
      if (channels[i] != 0)
      {
        pluggedChannels.Add(i + 1); // Convert to 1-based
      }
    }
    return pluggedChannels.ToArray();
  }

  /// <summary>
  /// Get board type for a specific channel
  /// </summary>
  /// <param name="deviceId">Device identifier</param>
  /// <param name="channel">Channel number (1-based: 1..16)</param>
  /// <returns>Board type (1=ESSENTIAL, 2=PREMIUM, 3=DIGICORE)</returns>
  /// <remarks>
  /// This API retrieves the actual board type from the hardware channel.
  /// It should be called after the device is connected to determine which firmware to load.
  /// </remarks>
  public static BOARD_TYPE GetChannelBoardType(int deviceId, int channel)
  {
    byte channelByte = (byte)(channel - 1); // Convert to 0-based
    int errorCode = ECLibNative.BL_GetChannelBoardType(deviceId, channelByte, out uint boardType);
    CheckError(errorCode, $"GetChannelBoardType for device {deviceId}, channel {channel}");
    
    return boardType switch
    {
      1 => BOARD_TYPE.ESSENTIAL,
      2 => BOARD_TYPE.PREMIUM,
      3 => BOARD_TYPE.DIGICORE,
      _ => BOARD_TYPE.UNKNOWN
    };
  }

  /// <summary>
  /// Drain pending channel messages without throwing on diagnostic failures.
  /// </summary>
  public static IReadOnlyList<string> DrainChannelMessages(int deviceId, int channel, int maxMessages = 16)
  {
    var messages = new List<string>();

    if (channel < 1 || channel > 16)
    {
      return messages;
    }

    byte channelByte = (byte)(channel - 1);
    for (int i = 0; i < maxMessages; i++)
    {
      uint size = 1024;
      var buffer = new StringBuilder((int)size);
      int errorCode = ECLibNative.BL_GetMessage(deviceId, channelByte, buffer, ref size);
      if (errorCode != 0)
      {
        break;
      }

      string message = buffer.ToString().TrimEnd('\0', ' ', '\r', '\n', '\t');
      if (size == 0 || string.IsNullOrWhiteSpace(message))
      {
        break;
      }

      messages.Add(message);
    }

    return messages;
  }

  /// <summary>
  /// Try to retrieve option error information for diagnostics.
  /// </summary>
  public static (int OptError, int OptPos)? TryGetOptionError(int deviceId, int channel)
  {
    if (channel < 1 || channel > 16)
    {
      return null;
    }

    byte channelByte = (byte)(channel - 1);
    int errorCode = ECLibNative.BL_GetOptErr(deviceId, channelByte, out int optError, out int optPos);
    if (errorCode != 0)
    {
      return null;
    }

    return (optError, optPos);
  }

  /// <summary>
  /// Get error message for error code
  /// </summary>
  /// <param name="errorCode">Error code</param>
  /// <returns>Error message string</returns>
  public static string GetErrorMessage(int errorCode)
  {
    // Check if this is a known error code
    if (Enum.IsDefined(typeof(BL_ERROR), errorCode))
    {
      var errorEnum = (BL_ERROR)errorCode;
      
      // For firmware not loaded error, return clear message without calling native function
      // This prevents access violations in BL_GetErrorMsg for certain error codes
      if (errorEnum == BL_ERROR.FIRM_FIRMWARENOTLOADED)
      {
        return "Firmware not loaded on channel. Please load firmware before accessing channel information.";
      }
      
      // Try to get message from native library
      try
      {
        var errorMsg = new StringBuilder(256);
        int result = ECLibNative.BL_GetErrorMsg(errorCode, errorMsg, errorMsg.Capacity);
        
        if (result == 0 && errorMsg.Length > 0)
        {
          return errorMsg.ToString().TrimEnd('\0');
        }
      }
      catch (AccessViolationException)
      {
        // Native library cannot handle this error code - fall through to enum-based message
      }
      
      // Return enum-based message as fallback
      string enumName = errorEnum.ToString().Replace('_', ' ');
      return $"{enumName} (Error code: {errorCode})";
    }
    
    // Unknown error code - try native library
    try
    {
      var errorMsg = new StringBuilder(256);
      ECLibNative.BL_GetErrorMsg(errorCode, errorMsg, errorMsg.Capacity);
      string message = errorMsg.ToString().TrimEnd('\0');
      return string.IsNullOrWhiteSpace(message) ? $"Unknown error code: {errorCode}" : message;
    }
    catch (AccessViolationException)
    {
      return $"Unknown error code: {errorCode} (error message unavailable)";
    }
  }

  #endregion

  #region Firmware Management

  /// <summary>
  /// Load firmware to specified channels
  /// </summary>
  /// <param name="deviceId">Device identifier</param>
  /// <param name="channels">Array of channel indices to load firmware on (1-based)</param>
  /// <param name="showGauge">Show progress gauge</param>
  /// <param name="force">Force firmware reload</param>
  /// <param name="firmwarePath">Path to firmware file relative to working directory (null for default)</param>
  /// <param name="fpgaPath">Path to FPGA file relative to working directory (null for default)</param>
  /// <returns>Dictionary mapping channel indices to result codes</returns>
  /// <remarks>
  /// Firmware paths should be relative to the current working directory.
  /// The working directory is automatically set to the setting.json location by App.xaml.cs.
  /// Typically, firmware files are located in the directory specified by TechniquesPath in setting.json.
  /// Example: If TechniquesPath is "lib", firmware files should be in "lib/kernel4.bin", "lib/Vmp_iv_0437_aa.xlx".
  /// </remarks>
  public static Dictionary<int, int> LoadFirmware(
    int deviceId,
    int[] channels,
    bool showGauge = true,
    bool force = false,
    string? firmwarePath = null,
    string? fpgaPath = null)
  {
    byte[] channelMap = new byte[16];
    foreach (int ch in channels)
    {
      if (ch >= 1 && ch <= 16)
      {
        channelMap[ch - 1] = 1; // Convert to 0-based and match c_bool[16]
      }
    }

    Log.Information("ECLibApi.LoadFirmware:");
    Log.Information("  Working directory: {Dir}", Directory.GetCurrentDirectory());
    Log.Information("  Kernel path:       {Path}", firmwarePath ?? "(null - using default)");
    Log.Information("  FPGA path:         {Path}", fpgaPath ?? "(null - using default)");

    int[] results = new int[16];
    int errorCode = ECLibNative.BL_LoadFirmware(
      deviceId,
      channelMap,
      results,
      (byte)results.Length,
      showGauge,
      force,
      firmwarePath,
      fpgaPath);

    CheckError(errorCode, $"LoadFirmware on device {deviceId}");

    // Build result dictionary (1-based channels)
    var resultDict = new Dictionary<int, int>();
    for (int i = 0; i < channels.Length; i++)
    {
      int ch = channels[i];
      if (ch >= 1 && ch <= 16)
      {
        resultDict[ch] = results[ch - 1];
      }
    }
    return resultDict;
  }

  #endregion

  #region Technique Management

  /// <summary>
  /// Define a boolean technique parameter using the vendor API.
  /// </summary>
  public static EccParam DefineBoolParameter(string label, bool value, int index = 0)
  {
    int errorCode = ECLibNative.BL_DefineBoolParameter(label, value, index, out EccParam parameter);
    CheckError(errorCode, $"DefineBoolParameter '{label}'");
    return parameter;
  }

  /// <summary>
  /// Define an integer technique parameter using the vendor API.
  /// </summary>
  public static EccParam DefineIntParameter(string label, int value, int index = 0)
  {
    int errorCode = ECLibNative.BL_DefineIntParameter(label, value, index, out EccParam parameter);
    CheckError(errorCode, $"DefineIntParameter '{label}'");
    return parameter;
  }

  /// <summary>
  /// Define a single-precision technique parameter using the vendor API.
  /// </summary>
  public static EccParam DefineSingleParameter(string label, float value, int index = 0)
  {
    int errorCode = ECLibNative.BL_DefineSglParameter(label, value, index, out EccParam parameter);
    CheckError(errorCode, $"DefineSingleParameter '{label}'");
    return parameter;
  }

  /// <summary>
  /// Load technique from .ecc file
  /// </summary>
  public static void LoadTechnique(
    int deviceId,
    int channel,
    string eccFilePath,
    ref EccParams parameters,
    bool first = true,
    bool last = true,
    bool display = false)
  {
    byte channelByte = (byte)(channel - 1); // Convert to 0-based
    int errorCode = ECLibNative.BL_LoadTechnique(
      deviceId,
      channelByte,
      eccFilePath,
      ref parameters,
      first,
      last,
      display);
    CheckError(errorCode, $"LoadTechnique for device {deviceId}, channel {channel}");
  }

  #endregion

  #region Channel Control

  /// <summary>
  /// Start channel execution
  /// </summary>
  public static void StartChannel(int deviceId, int channel)
  {
    byte channelByte = (byte)(channel - 1); // Convert to 0-based
    int errorCode = ECLibNative.BL_StartChannel(deviceId, channelByte);
    CheckError(errorCode, $"StartChannel for device {deviceId}, channel {channel}");
  }

  /// <summary>
  /// Start multiple channels
  /// </summary>
  public static Dictionary<int, int> StartChannels(int deviceId, int[] channels)
  {
    byte[] channelMap = new byte[16];
    foreach (int ch in channels)
    {
      if (ch >= 1 && ch <= 16)
      {
        channelMap[ch - 1] = 1;
      }
    }

    int[] results = new int[16];
    int errorCode = ECLibNative.BL_StartChannels(deviceId, channelMap, results, (byte)results.Length);
    CheckError(errorCode, $"StartChannels for device {deviceId}");

    var resultDict = new Dictionary<int, int>();
    foreach (int ch in channels)
    {
      if (ch >= 1 && ch <= 16)
      {
        resultDict[ch] = results[ch - 1];
      }
    }
    return resultDict;
  }

  /// <summary>
  /// Stop channel execution
  /// </summary>
  public static void StopChannel(int deviceId, int channel)
  {
    byte channelByte = (byte)(channel - 1); // Convert to 0-based
    int errorCode = ECLibNative.BL_StopChannel(deviceId, channelByte);
    CheckError(errorCode, $"StopChannel for device {deviceId}, channel {channel}");
  }

  /// <summary>
  /// Stop multiple channels
  /// </summary>
  public static Dictionary<int, int> StopChannels(int deviceId, int[] channels)
  {
    byte[] channelMap = new byte[16];
    foreach (int ch in channels)
    {
      if (ch >= 1 && ch <= 16)
      {
        channelMap[ch - 1] = 1;
      }
    }

    int[] results = new int[16];
    int errorCode = ECLibNative.BL_StopChannels(deviceId, channelMap, results, (byte)results.Length);
    CheckError(errorCode, $"StopChannels for device {deviceId}");

    var resultDict = new Dictionary<int, int>();
    foreach (int ch in channels)
    {
      if (ch >= 1 && ch <= 16)
      {
        resultDict[ch] = results[ch - 1];
      }
    }
    return resultDict;
  }

  #endregion

  #region Data Acquisition

  /// <summary>
  /// Get current values from channel
  /// </summary>
  public static CurrentValues GetCurrentValues(int deviceId, int channel)
  {
    byte channelByte = (byte)(channel - 1); // Convert to 0-based
    int errorCode = ECLibNative.BL_GetCurrentValues(deviceId, channelByte, out CurrentValues values);
    CheckError(errorCode, $"GetCurrentValues for device {deviceId}, channel {channel}");
    return values;
  }

  /// <summary>
  /// Get data from channel
  /// </summary>
  public static (CurrentValues CurrentValues, DataInfo DataInfo, uint[] DataBuffer) GetData(int deviceId, int channel)
  {
    byte channelByte = (byte)(channel - 1); // Convert to 0-based
    uint[] dataBuffer = new uint[1000];
    int errorCode = ECLibNative.BL_GetData(
      deviceId,
      channelByte,
      dataBuffer,
      out DataInfo dataInfo,
      out CurrentValues currentValues);
    CheckError(errorCode, $"GetData for device {deviceId}, channel {channel}");
    return (currentValues, dataInfo, dataBuffer);
  }

  /// <summary>
  /// Convert numeric value to single (float)
  /// </summary>
  public static float ConvertNumericIntoSingle(uint numericValue)
  {
    int errorCode = ECLibNative.BL_ConvertNumericIntoSingle(numericValue, out float floatValue);
    CheckError(errorCode, "ConvertNumericIntoSingle");
    return floatValue;
  }

  /// <summary>
  /// Convert a channel-specific numeric value returned by BL_GetData into a single.
  /// </summary>
  public static float ConvertChannelNumericIntoSingle(uint numericValue, uint boardType)
  {
    int errorCode = ECLibNative.BL_ConvertChannelNumericIntoSingle(numericValue, out float floatValue, boardType);
    CheckError(errorCode, "ConvertChannelNumericIntoSingle");
    return floatValue;
  }

  /// <summary>
  /// Convert the two-word channel time value returned by BL_GetData into seconds.
  /// </summary>
  public static double ConvertTimeChannelNumericIntoSeconds(uint highValue, uint lowValue, float timeBase, uint boardType)
  {
    uint[] timeData = [highValue, lowValue];
    int errorCode = ECLibNative.BL_ConvertTimeChannelNumericIntoSeconds(timeData, out double timeSeconds, timeBase, boardType);
    CheckError(errorCode, "ConvertTimeChannelNumericIntoSeconds");
    return timeSeconds;
  }

  #endregion

  #region Hardware Configuration

  /// <summary>
  /// Get hardware configuration (VMP300 family only)
  /// </summary>
  public static HardwareConf GetHardwareConfiguration(int deviceId, int channel)
  {
    byte channelByte = (byte)(channel - 1); // Convert to 0-based
    int errorCode = ECLibNative.BL_GetHardConf(deviceId, channelByte, out HardwareConf conf);
    CheckError(errorCode, $"GetHardwareConfiguration for device {deviceId}, channel {channel}");
    return conf;
  }

  /// <summary>
  /// Set hardware configuration (VMP300 family only)
  /// </summary>
  public static void SetHardwareConfiguration(
    int deviceId,
    int channel,
    HW_CNX connection,
    HW_MODE mode)
  {
    byte channelByte = (byte)(channel - 1); // Convert to 0-based
    int errorCode = ECLibNative.BL_SetHardConf(deviceId, channelByte, (int)connection, (int)mode);
    CheckError(errorCode, $"SetHardwareConfiguration for device {deviceId}, channel {channel}");
  }

  #endregion

  #region Communication Speed

  /// <summary>
  /// Test communication speed
  /// </summary>
  public static (int ReceiverSpeed, int FirmwareSpeed) TestCommSpeed(int deviceId, int channel)
  {
    byte channelByte = (byte)(channel - 1); // Convert to 0-based
    int errorCode = ECLibNative.BL_TestCommSpeed(
      deviceId,
      channelByte,
      out int rcvtSpeed,
      out int firmwareSpeed);
    CheckError(errorCode, $"TestCommSpeed for device {deviceId}, channel {channel}");
    return (rcvtSpeed, firmwareSpeed);
  }

  #endregion

  #region Error Handling

  private static void CheckError(int errorCode, string context)
  {
    if (errorCode != 0)
    {
      string errorMessage;
      
      try
      {
        errorMessage = GetErrorMessage(errorCode);
      }
      catch (Exception ex)
      {
        // If getting error message fails, create a basic message
        errorMessage = $"Error code {errorCode} (failed to retrieve message: {ex.Message})";
      }
      
      throw new ECLibException(errorCode, context, errorMessage);
    }
  }

  #endregion
}

/// <summary>
/// Exception thrown by ECLibApi methods when an error occurs
/// </summary>
public class ECLibException : Exception
{
  public int ErrorCode { get; }
  public string Context { get; }

  /// <summary>
  /// Get the error code as BL_ERROR enum if it's a known error, otherwise null
  /// </summary>
  public BL_ERROR? ErrorType
  {
    get
    {
      if (Enum.IsDefined(typeof(BL_ERROR), this.ErrorCode))
      {
        return (BL_ERROR)this.ErrorCode;
      }
      return null;
    }
  }

  public ECLibException(int errorCode, string context, string message)
    : base($"{context}: {message} (Error code: {errorCode})")
  {
    this.ErrorCode = errorCode;
    this.Context = context;
  }
}
