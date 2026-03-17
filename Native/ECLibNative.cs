using System.Runtime.InteropServices;
using System.Reflection;
using System.Text;

namespace Biologic.Native;

/// <summary>
/// P/Invoke declarations for EClib64.dll - BioLogic EC-Lab Development Package
/// </summary>
/// <remarks>
/// <para>
/// This class contains low-level P/Invoke declarations matching the EClib64.dll API.
/// These methods are marked as internal to prevent direct external access (CA1401 compliance).
/// </para>
/// <para>
/// For type-safe, user-friendly API, use <see cref="ECLibApi"/> instead.
/// </para>
/// <para>
/// Reference documentation:
/// - C:\Program Files (x86)\EC-Lab Development Package\Development Package.pdf
/// - kbio_api.py from BioLogic OEM Package Python examples
/// </para>
/// </remarks>
internal static class ECLibNative
{
  private const string DllName = "EClib64.dll";
    private static readonly object ResolverSync = new();
    private static string? preferredLibraryDirectory;
    private static IntPtr preferredLibraryHandle;
    private static bool resolverRegistered;

    internal static void ConfigurePreferredLibraryDirectory(string libraryDirectory)
    {
        if (string.IsNullOrWhiteSpace(libraryDirectory) || !Directory.Exists(libraryDirectory))
        {
            return;
        }

        lock (ResolverSync)
        {
            preferredLibraryDirectory = Path.GetFullPath(libraryDirectory);

            if (!resolverRegistered)
            {
                NativeLibrary.SetDllImportResolver(typeof(ECLibNative).Assembly, ResolveLibraryImport);
                resolverRegistered = true;
            }

            string preferredLibraryPath = Path.Combine(preferredLibraryDirectory, DllName);
            if (File.Exists(preferredLibraryPath))
            {
                preferredLibraryHandle = NativeLibrary.Load(preferredLibraryPath);
            }
        }
    }

    private static IntPtr ResolveLibraryImport(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (!string.Equals(libraryName, DllName, StringComparison.OrdinalIgnoreCase))
        {
            return IntPtr.Zero;
        }

        lock (ResolverSync)
        {
            if (preferredLibraryHandle != IntPtr.Zero)
            {
                return preferredLibraryHandle;
            }

            if (!string.IsNullOrWhiteSpace(preferredLibraryDirectory))
            {
                string preferredLibraryPath = Path.Combine(preferredLibraryDirectory, DllName);
                if (File.Exists(preferredLibraryPath))
                {
                    preferredLibraryHandle = NativeLibrary.Load(preferredLibraryPath);
                    return preferredLibraryHandle;
                }
            }
        }

        return IntPtr.Zero;
    }

  #region Connection Management

  /// <summary>
  /// Connect to a BioLogic device
  /// </summary>
  /// <param name="address">Device address (e.g., "USB0", "192.168.1.100")</param>
  /// <param name="timeout">Connection timeout in seconds</param>
  /// <param name="id">Output device identifier</param>
  /// <param name="deviceInfo">Output device information</param>
  /// <returns>Error code (0 = success)</returns>
  [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
  internal static extern int BL_Connect(
      [MarshalAs(UnmanagedType.LPStr)] string address,
      byte timeout,
      out int id,
      out DeviceInfo deviceInfo);

  /// <summary>
  /// Disconnect from a device
  /// </summary>
  /// <param name="id">Device identifier</param>
  /// <returns>Error code (0 = success)</returns>
  [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
  internal static extern int BL_Disconnect(int id);

  /// <summary>
  /// Test connection to a device
  /// </summary>
  /// <param name="id">Device identifier</param>
  /// <returns>Error code (0 = success)</returns>
  [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
  internal static extern int BL_TestConnection(int id);

  /// <summary>
  /// Test communication speed with device
  /// </summary>
  /// <param name="id">Device identifier</param>
  /// <param name="channel">Channel index (0-based)</param>
  /// <param name="rcvtSpeed">Received speed</param>
  /// <param name="kernelSpeed">Kernel speed</param>
  /// <returns>Error code (0 = success)</returns>
  [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
  internal static extern int BL_TestCommSpeed(
      int id,
      byte channel,
      out int rcvtSpeed,
      out int kernelSpeed);

  #endregion

  #region Device Information

  /// <summary>
  /// Get library version
  /// </summary>
  /// <param name="versionStr">Output version string buffer</param>
  /// <param name="size">Pointer to buffer size (input: max size, output: actual size)</param>
  /// <returns>Error code (0 = success)</returns>
  [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
  internal static extern int BL_GetLibVersion(
      StringBuilder versionStr,
      ref uint size);

  /// <summary>
  /// Get USB device information by index
  /// </summary>
  [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
  [return: MarshalAs(UnmanagedType.Bool)]
  internal static extern bool BL_GetUSBdeviceinfos(
      int index,
      StringBuilder company,
      int companySize,
      StringBuilder device,
      int deviceSize,
      StringBuilder serialNumber,
      int serialSize);

  /// <summary>
  /// Get channel information
  /// </summary>
  /// <param name="id">Device identifier</param>
  /// <param name="channel">Channel index (0-based)</param>
  /// <param name="channelInfo">Output channel information</param>
  /// <returns>Error code (0 = success)</returns>
  [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
  internal static extern int BL_GetChannelInfos(
      int id,
      byte channel,
      out ChannelInfo channelInfo);

  /// <summary>
  /// Get which channels are plugged
  /// </summary>
  /// <param name="id">Device identifier</param>
  /// <param name="channels">Output channel array (16 bytes)</param>
  /// <param name="size">Array size</param>
  /// <returns>Error code (0 = success)</returns>
  [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
  internal static extern int BL_GetChannelsPlugged(
      int id,
      [MarshalAs(UnmanagedType.LPArray, SizeConst = 16)] byte[] channels,
      byte size);

  /// <summary>
  /// Get error message for error code
  /// </summary>
  /// <param name="errorCode">Error code</param>
  /// <param name="message">Output message buffer</param>
  /// <param name="size">Buffer size</param>
  /// <returns>Error code (0 = success)</returns>
  [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
  internal static extern int BL_GetErrorMsg(
      int errorCode,
      StringBuilder message,
      int size);

  /// <summary>
  /// Get pending firmware/channel message text.
  /// </summary>
  [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
  internal static extern int BL_GetMessage(
      int id,
      byte channel,
      StringBuilder message,
      ref uint size);

  /// <summary>
  /// Get current option error information for a channel.
  /// </summary>
  [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
  internal static extern int BL_GetOptErr(
      int id,
      byte channel,
      out int optError,
      out int optPos);

  #endregion

  #region Firmware Management

  /// <summary>
  /// Get short path name (8.3 format) for better compatibility with native libraries
  /// </summary>
  [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
  private static extern int GetShortPathName(
      [MarshalAs(UnmanagedType.LPTStr)] string lpszLongPath,
      [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpszShortPath,
      int cchBuffer);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int GetModuleFileName(IntPtr hModule, StringBuilder lpFilename, int nSize);

  /// <summary>
  /// Load firmware to specified channels
  /// </summary>
  [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
  internal static extern int BL_LoadFirmware(
      int id,
      [MarshalAs(UnmanagedType.LPArray, SizeConst = 16)] byte[] channels,
      [MarshalAs(UnmanagedType.LPArray, SizeConst = 16)] int[] results,
      byte length,
      [MarshalAs(UnmanagedType.Bool)] bool showGauge,
      [MarshalAs(UnmanagedType.Bool)] bool forceReload,
      [MarshalAs(UnmanagedType.LPStr)] string? firmwarePath,
      [MarshalAs(UnmanagedType.LPStr)] string? fpgaPath);

  /// <summary>
  /// Convert long path to short path (8.3 format) for better compatibility
  /// </summary>
  internal static string GetShortPath(string longPath)
  {
    if (string.IsNullOrEmpty(longPath))
      return longPath;

    StringBuilder shortPath = new StringBuilder(260);
    int result = GetShortPathName(longPath, shortPath, shortPath.Capacity);
    
    if (result == 0)
    {
      // Failed to get short path - log the error and return original
      int errorCode = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
      System.Diagnostics.Debug.WriteLine($"GetShortPathName failed for '{longPath}' with error {errorCode}");
      return longPath;
    }
    
    if (result > shortPath.Capacity)
    {
      // Buffer too small - retry with larger buffer
      shortPath = new StringBuilder(result);
      result = GetShortPathName(longPath, shortPath, shortPath.Capacity);
      
      if (result == 0 || result > shortPath.Capacity)
      {
        System.Diagnostics.Debug.WriteLine($"GetShortPathName failed for '{longPath}' on retry");
        return longPath;
      }
    }
    
    return shortPath.ToString();
  }

    internal static string? GetLoadedModulePath(string moduleName)
    {
        IntPtr moduleHandle = GetModuleHandle(moduleName);
        if (moduleHandle == IntPtr.Zero)
        {
            return null;
        }

        StringBuilder buffer = new StringBuilder(260);
        int result = GetModuleFileName(moduleHandle, buffer, buffer.Capacity);
        if (result == 0)
        {
            return null;
        }

        if (result >= buffer.Capacity)
        {
            buffer = new StringBuilder(1024);
            result = GetModuleFileName(moduleHandle, buffer, buffer.Capacity);
            if (result == 0)
            {
                return null;
            }
        }

        return buffer.ToString();
    }

  /// <summary>
  /// Load flash firmware
  /// </summary>
  [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
  internal static extern int BL_LoadFlash(
      int id,
      [MarshalAs(UnmanagedType.LPStr)] string firmwarePath,
      [MarshalAs(UnmanagedType.Bool)] bool showGauge);

  #endregion

  #region Hardware Configuration

  /// <summary>
  /// Get hardware configuration
  /// </summary>
  [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
  internal static extern int BL_GetHardConf(
      int id,
      byte channel,
      out HardwareConf hardwareConf);

  /// <summary>
  /// Set hardware configuration
  /// </summary>
  [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
  internal static extern int BL_SetHardConf(
      int id,
      byte channel,
      HardwareConf hardwareConf);

  /// <summary>
  /// Set hardware configuration (alternative signature with separate parameters)
  /// </summary>
  [DllImport(DllName, EntryPoint = "BL_SetHardConf", CallingConvention = CallingConvention.StdCall)]
  internal static extern int BL_SetHardConf(
      int id,
      byte channel,
      int connection,
      int mode);

  #endregion

  #region Technique Management

  /// <summary>
  /// Load technique from .ecc file
  /// </summary>
  [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
  internal static extern int BL_LoadTechnique(
      int id,
      byte channel,
      [MarshalAs(UnmanagedType.LPStr)] string eccFile,
      ref EccParams parameters,
      [MarshalAs(UnmanagedType.Bool)] bool first,
      [MarshalAs(UnmanagedType.Bool)] bool last,
      [MarshalAs(UnmanagedType.Bool)] bool display);

  /// <summary>
  /// Define boolean parameter for technique
  /// </summary>
  [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
  internal static extern int BL_DefineBoolParameter(
      [MarshalAs(UnmanagedType.LPStr)] string label,
      [MarshalAs(UnmanagedType.Bool)] bool value,
      int index,
      out EccParam parameter);

  /// <summary>
  /// Define integer parameter for technique
  /// </summary>
  [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
  internal static extern int BL_DefineIntParameter(
      [MarshalAs(UnmanagedType.LPStr)] string label,
      int value,
      int index,
      out EccParam parameter);

  /// <summary>
  /// Define single (float) parameter for technique
  /// </summary>
  [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
  internal static extern int BL_DefineSglParameter(
      [MarshalAs(UnmanagedType.LPStr)] string label,
      float value,
      int index,
      out EccParam parameter);

  /// <summary>
  /// Update technique parameters
  /// </summary>
  [DllImport(DllName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
  internal static extern int BL_UpdateParameters(
      int id,
      byte channel,
      int index,
      ref EccParams parameters,
      [MarshalAs(UnmanagedType.LPStr)] string eccFile);

  #endregion

  #region Channel Control

  /// <summary>
  /// Start a single channel
  /// </summary>
  [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
  internal static extern int BL_StartChannel(int id, byte channel);

  /// <summary>
  /// Start multiple channels
  /// </summary>
  [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
  internal static extern int BL_StartChannels(
      int id,
      [MarshalAs(UnmanagedType.LPArray, SizeConst = 16)] byte[] channels,
      [MarshalAs(UnmanagedType.LPArray, SizeConst = 16)] int[] results,
      byte length);

  /// <summary>
  /// Stop a single channel
  /// </summary>
  [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
  internal static extern int BL_StopChannel(int id, byte channel);

  /// <summary>
  /// Stop multiple channels
  /// </summary>
  [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
  internal static extern int BL_StopChannels(
      int id,
      [MarshalAs(UnmanagedType.LPArray, SizeConst = 16)] byte[] channels,
      [MarshalAs(UnmanagedType.LPArray, SizeConst = 16)] int[] results,
      byte length);

  #endregion

  #region Data Acquisition

  /// <summary>
  /// Get current values from channel
  /// </summary>
  [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
  internal static extern int BL_GetCurrentValues(
      int id,
      byte channel,
      out CurrentValues currentValues);

  /// <summary>
  /// Get data from channel
  /// </summary>
  [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
  internal static extern int BL_GetData(
      int id,
      byte channel,
      [MarshalAs(UnmanagedType.LPArray, SizeConst = 1000)] uint[] dataBuffer,
      out DataInfo dataInfo,
      out CurrentValues currentValues);

  /// <summary>
  /// Convert numeric value to single (float)
  /// </summary>
  [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
  internal static extern int BL_ConvertNumericIntoSingle(
      uint numericValue,
      out float floatValue);

  /// <summary>
  /// Convert channel-specific numeric value to single (float)
  /// </summary>
  [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
  internal static extern int BL_ConvertChannelNumericIntoSingle(
      uint numericValue,
      out float floatValue,
      uint boardType);

  /// <summary>
  /// Convert time numeric values to seconds
  /// </summary>
  [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
  internal static extern int BL_ConvertTimeChannelNumericIntoSeconds(
      [MarshalAs(UnmanagedType.LPArray)] uint[] timeData,
      out double timeSeconds,
      float timeBase,
      uint boardType);

  /// <summary>
  /// Get channel board type
  /// </summary>
  [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
  internal static extern int BL_GetChannelBoardType(
      int id,
      byte channel,
      out uint boardType);

  #endregion

  #region Helper Methods

  /// <summary>
  /// Helper method to get error message from error code
  /// </summary>
  internal static string GetErrorMessage(int errorCode)
  {
    var errorMsg = new StringBuilder(256);
    BL_GetErrorMsg(errorCode, errorMsg, errorMsg.Capacity);
    return errorMsg.ToString().TrimEnd('\0');
  }

  /// <summary>
  /// Helper method to create channel array for multi-channel operations
  /// </summary>
  internal static bool[] CreateChannelArray(params int[] channelIndices)
  {
    bool[] channels = new bool[16];
    foreach (int ch in channelIndices)
    {
      if (ch >= 1 && ch <= 16)
      {
        channels[ch - 1] = true; // Convert to 0-based
      }
    }
    return channels;
  }

  /// <summary>
  /// Helper method to create EccParam array
  /// </summary>
  internal static EccParam[] CreateEccParamArray(int size)
  {
    EccParam[] parameters = new EccParam[size];
    for (int i = 0; i < size; i++)
    {
      parameters[i].ParamStr = new byte[64];
    }
    return parameters;
  }

  #endregion
}
