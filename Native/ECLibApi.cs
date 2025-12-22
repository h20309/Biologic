using System.Text;

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
    bool showProgress = false,
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
  /// <returns>Tuple containing version code and version string</returns>
  public static (int Version, string VersionString) GetLibVersion()
  {
    var versionStr = new StringBuilder(256);
    int errorCode = ECLibNative.BL_GetLibVersion(out int version, versionStr, versionStr.Capacity);
    CheckError(errorCode, "GetLibVersion");
    return (version, versionStr.ToString().TrimEnd('\0'));
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
    var pluggedChannels = new System.Collections.Generic.List<int>();
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
  /// Get error message for error code
  /// </summary>
  /// <param name="errorCode">Error code</param>
  /// <returns>Error message string</returns>
  public static string GetErrorMessage(int errorCode)
  {
    var errorMsg = new StringBuilder(256);
    ECLibNative.BL_GetErrorMsg(errorCode, errorMsg, errorMsg.Capacity);
    return errorMsg.ToString().TrimEnd('\0');
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
  /// <param name="firmwarePath">Path to firmware file (null for default)</param>
  /// <param name="fpgaPath">Path to FPGA file (null for default)</param>
  /// <returns>Dictionary mapping channel indices to result codes</returns>
  public static System.Collections.Generic.Dictionary<int, int> LoadFirmware(
    int deviceId,
    int[] channels,
    bool showGauge = true,
    bool force = false,
    string firmwarePath = null,
    string fpgaPath = null)
  {
    bool[] channelMap = new bool[16];
    foreach (int ch in channels)
    {
      if (ch >= 1 && ch <= 16)
      {
        channelMap[ch - 1] = true; // Convert to 0-based
      }
    }

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
    var resultDict = new System.Collections.Generic.Dictionary<int, int>();
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
  public static System.Collections.Generic.Dictionary<int, int> StartChannels(int deviceId, int[] channels)
  {
    bool[] channelMap = new bool[16];
    foreach (int ch in channels)
    {
      if (ch >= 1 && ch <= 16)
      {
        channelMap[ch - 1] = true;
      }
    }

    int[] results = new int[16];
    int errorCode = ECLibNative.BL_StartChannels(deviceId, channelMap, results, (byte)results.Length);
    CheckError(errorCode, $"StartChannels for device {deviceId}");

    var resultDict = new System.Collections.Generic.Dictionary<int, int>();
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
  public static System.Collections.Generic.Dictionary<int, int> StopChannels(int deviceId, int[] channels)
  {
    bool[] channelMap = new bool[16];
    foreach (int ch in channels)
    {
      if (ch >= 1 && ch <= 16)
      {
        channelMap[ch - 1] = true;
      }
    }

    int[] results = new int[16];
    int errorCode = ECLibNative.BL_StopChannels(deviceId, channelMap, results, (byte)results.Length);
    CheckError(errorCode, $"StopChannels for device {deviceId}");

    var resultDict = new System.Collections.Generic.Dictionary<int, int>();
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
      string errorMessage = GetErrorMessage(errorCode);
      throw new ECLibException(errorCode, context, errorMessage);
    }
  }

  #endregion
}

/// <summary>
/// Exception thrown by ECLibApi methods when an error occurs
/// </summary>
public class ECLibException : System.Exception
{
  public int ErrorCode { get; }
  public string Context { get; }

  public ECLibException(int errorCode, string context, string message)
    : base($"{context}: {message} (Error code: {errorCode})")
  {
    this.ErrorCode = errorCode;
    this.Context = context;
  }
}
