using System.Text;

namespace Biologic.Native;

/// <summary>
/// Abstraction over the low-level EClib64.dll P/Invoke surface.
/// Implementations: <see cref="RealECLibNative"/> (hardware) and <see cref="MockECLibNative"/> (simulation).
/// </summary>
internal interface IECLibNative
{
  // Connection Management

  int BL_Connect(string address, byte timeout, out int id, out DeviceInfo deviceInfo);

  int BL_Disconnect(int id);

  int BL_TestConnection(int id);

  int BL_TestCommSpeed(int id, byte channel, out int rcvtSpeed, out int kernelSpeed);

  // Device Information

  int BL_GetLibVersion(StringBuilder versionStr, ref uint size);

  int BL_GetChannelInfos(int id, byte channel, out ChannelInfo channelInfo);

  int BL_GetChannelsPlugged(int id, byte[] channels, byte size);

  int BL_GetErrorMsg(int errorCode, StringBuilder message, int size);

  int BL_GetMessage(int id, byte channel, StringBuilder message, ref uint size);

  int BL_GetOptErr(int id, byte channel, out int optError, out int optPos);

  int BL_GetChannelBoardType(int id, byte channel, out uint boardType);

  // Firmware Management

  int BL_LoadFirmware(
    int id,
    byte[] channels,
    int[] results,
    byte length,
    bool showGauge,
    bool forceReload,
    string? firmwarePath,
    string? fpgaPath);

  // Technique Management

  int BL_LoadTechnique(
    int id,
    byte channel,
    string eccFile,
    ref EccParams parameters,
    bool first,
    bool last,
    bool display);

  int BL_DefineBoolParameter(string label, bool value, int index, out EccParam parameter);

  int BL_DefineIntParameter(string label, int value, int index, out EccParam parameter);

  int BL_DefineSglParameter(string label, float value, int index, out EccParam parameter);

  int BL_UpdateParameters(
    int id,
    byte channel,
    int index,
    ref EccParams parameters,
    string eccFile);

  int BL_GetTechniqueInfos(int id, byte channel, int index, out TechniqueInfos infos);

  int BL_GetParamInfos(int id, byte channel, int index, out TechniqueInfos infos);

  // Channel Control

  int BL_StartChannel(int id, byte channel);

  int BL_StartChannels(int id, byte[] channels, int[] results, byte length);

  int BL_StopChannel(int id, byte channel);

  int BL_StopChannels(int id, byte[] channels, int[] results, byte length);

  // Data Acquisition

  int BL_GetCurrentValues(int id, byte channel, out CurrentValues currentValues);

  int BL_GetData(int id, byte channel, uint[] dataBuffer, out DataInfo dataInfo, out CurrentValues currentValues);

  int BL_ConvertNumericIntoSingle(uint numericValue, out float floatValue);

  int BL_ConvertChannelNumericIntoSingle(uint numericValue, out float floatValue, uint boardType);

  int BL_ConvertTimeChannelNumericIntoSeconds(uint[] timeData, out double timeSeconds, float timeBase, uint boardType);

  // Hardware Configuration

  int BL_GetHardConf(int id, byte channel, out HardwareConf hardwareConf);

  int BL_SetHardConf(int id, byte channel, int connection, int mode);

  // Utility

  string GetShortPath(string longPath);

  string? GetLoadedModulePath(string moduleName);
}
