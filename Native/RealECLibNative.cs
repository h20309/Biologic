using System.Text;

namespace Biologic.Native;

/// <summary>
/// Production adapter that delegates every call to the static <see cref="ECLibNative"/> P/Invoke surface.
/// </summary>
internal sealed class RealECLibNative : IECLibNative
{
  public int BL_Connect(string address, byte timeout, out int id, out DeviceInfo deviceInfo)
    => ECLibNative.BL_Connect(address, timeout, out id, out deviceInfo);

  public int BL_Disconnect(int id)
    => ECLibNative.BL_Disconnect(id);

  public int BL_TestConnection(int id)
    => ECLibNative.BL_TestConnection(id);

  public int BL_TestCommSpeed(int id, byte channel, out int rcvtSpeed, out int kernelSpeed)
    => ECLibNative.BL_TestCommSpeed(id, channel, out rcvtSpeed, out kernelSpeed);

  public int BL_GetLibVersion(StringBuilder versionStr, ref uint size)
    => ECLibNative.BL_GetLibVersion(versionStr, ref size);

  public int BL_GetChannelInfos(int id, byte channel, out ChannelInfo channelInfo)
    => ECLibNative.BL_GetChannelInfos(id, channel, out channelInfo);

  public int BL_GetChannelsPlugged(int id, byte[] channels, byte size)
    => ECLibNative.BL_GetChannelsPlugged(id, channels, size);

  public int BL_GetErrorMsg(int errorCode, StringBuilder message, int size)
    => ECLibNative.BL_GetErrorMsg(errorCode, message, size);

  public int BL_GetMessage(int id, byte channel, StringBuilder message, ref uint size)
    => ECLibNative.BL_GetMessage(id, channel, message, ref size);

  public int BL_GetOptErr(int id, byte channel, out int optError, out int optPos)
    => ECLibNative.BL_GetOptErr(id, channel, out optError, out optPos);

  public int BL_GetChannelBoardType(int id, byte channel, out uint boardType)
    => ECLibNative.BL_GetChannelBoardType(id, channel, out boardType);

  public int BL_LoadFirmware(int id, byte[] channels, int[] results, byte length, bool showGauge, bool forceReload, string? firmwarePath, string? fpgaPath)
    => ECLibNative.BL_LoadFirmware(id, channels, results, length, showGauge, forceReload, firmwarePath, fpgaPath);

  public int BL_LoadTechnique(int id, byte channel, string eccFile, ref EccParams parameters, bool first, bool last, bool display)
    => ECLibNative.BL_LoadTechnique(id, channel, eccFile, ref parameters, first, last, display);

  public int BL_DefineBoolParameter(string label, bool value, int index, out EccParam parameter)
    => ECLibNative.BL_DefineBoolParameter(label, value, index, out parameter);

  public int BL_DefineIntParameter(string label, int value, int index, out EccParam parameter)
    => ECLibNative.BL_DefineIntParameter(label, value, index, out parameter);

  public int BL_DefineSglParameter(string label, float value, int index, out EccParam parameter)
    => ECLibNative.BL_DefineSglParameter(label, value, index, out parameter);

  public int BL_UpdateParameters(int id, byte channel, int index, ref EccParams parameters, string eccFile)
    => ECLibNative.BL_UpdateParameters(id, channel, index, ref parameters, eccFile);

  public int BL_GetTechniqueInfos(int id, byte channel, int index, out TechniqueInfos infos)
    => ECLibNative.BL_GetTechniqueInfos(id, channel, index, out infos);

  public int BL_GetParamInfos(int id, byte channel, int index, out TechniqueInfos infos)
    => ECLibNative.BL_GetParamInfos(id, channel, index, out infos);

  public int BL_StartChannel(int id, byte channel)
    => ECLibNative.BL_StartChannel(id, channel);

  public int BL_StartChannels(int id, byte[] channels, int[] results, byte length)
    => ECLibNative.BL_StartChannels(id, channels, results, length);

  public int BL_StopChannel(int id, byte channel)
    => ECLibNative.BL_StopChannel(id, channel);

  public int BL_StopChannels(int id, byte[] channels, int[] results, byte length)
    => ECLibNative.BL_StopChannels(id, channels, results, length);

  public int BL_GetCurrentValues(int id, byte channel, out CurrentValues currentValues)
    => ECLibNative.BL_GetCurrentValues(id, channel, out currentValues);

  public int BL_GetData(int id, byte channel, uint[] dataBuffer, out DataInfo dataInfo, out CurrentValues currentValues)
    => ECLibNative.BL_GetData(id, channel, dataBuffer, out dataInfo, out currentValues);

  public int BL_ConvertNumericIntoSingle(uint numericValue, out float floatValue)
    => ECLibNative.BL_ConvertNumericIntoSingle(numericValue, out floatValue);

  public int BL_ConvertChannelNumericIntoSingle(uint numericValue, out float floatValue, uint boardType)
    => ECLibNative.BL_ConvertChannelNumericIntoSingle(numericValue, out floatValue, boardType);

  public int BL_ConvertTimeChannelNumericIntoSeconds(uint[] timeData, out double timeSeconds, float timeBase, uint boardType)
    => ECLibNative.BL_ConvertTimeChannelNumericIntoSeconds(timeData, out timeSeconds, timeBase, boardType);

  public int BL_GetHardConf(int id, byte channel, out HardwareConf hardwareConf)
    => ECLibNative.BL_GetHardConf(id, channel, out hardwareConf);

  public int BL_SetHardConf(int id, byte channel, int connection, int mode)
    => ECLibNative.BL_SetHardConf(id, channel, connection, mode);

  public string GetShortPath(string longPath)
    => ECLibNative.GetShortPath(longPath);

  public string? GetLoadedModulePath(string moduleName)
    => ECLibNative.GetLoadedModulePath(moduleName);
}
