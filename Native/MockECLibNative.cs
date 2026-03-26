using System.Text;
using Serilog;

namespace Biologic.Native;

/// <summary>
/// In-memory mock of the EClib64.dll native layer.
/// Simulates a single BioLogic VSP-3e device with configurable channels,
/// allowing the full system to run without hardware or the vendor DLL.
/// </summary>
internal sealed class MockECLibNative : IECLibNative
{
  private readonly object syncRoot = new();
  private readonly int channelCount;
  private readonly int techniqueRunDurationSeconds;
  private int nextDeviceId;

  private readonly Dictionary<int, MockDeviceState> devices = new();

  public MockECLibNative(int channelCount = 1, int techniqueRunDurationSeconds = 10)
  {
    this.channelCount = Math.Clamp(channelCount, 1, 16);
    this.techniqueRunDurationSeconds = Math.Max(1, techniqueRunDurationSeconds);
  }

  // ---- internal state types ----

  private sealed class MockDeviceState
  {
    public DeviceInfo Info;
    public Dictionary<byte, MockChannelState> Channels = new();
  }

  private sealed class MockChannelState
  {
    public PROG_STATE State = PROG_STATE.STOP;
    public string? LoadedTechnique;
    public DateTime StartTime;
    public bool FirmwareLoaded;
    public int DataRowIndex;

    // EIS-specific state
    public int EisProcessPhase; // 0 = warmup, 1 = spectrum, 2 = done
    public int EisWarmupEmitted;
    public int EisSpectrumEmitted;
    public int EisSpectrumTotal;
    public float EisInitialFrequency;
    public float EisFinalFrequency;
  }

  // ---- Connection Management ----

  public int BL_Connect(string address, byte timeout, out int id, out DeviceInfo deviceInfo)
  {
    lock (this.syncRoot)
    {
      id = this.nextDeviceId++;
      deviceInfo = new DeviceInfo
      {
        DeviceCode = (int)DEVICE.VSP3E,
        RAMSize = 65536,
        CPU = 1,
        NumberOfChannels = this.channelCount,
        NumberOfSlots = this.channelCount,
        FirmwareVersion = 1100,
        FirmwareDate_yyyy = 2025,
        FirmwareDate_mm = 1,
        FirmwareDate_dd = 1,
        HTdisplayOn = 0,
        NbOfConnectedPC = 1,
      };

      var device = new MockDeviceState { Info = deviceInfo };
      for (byte ch = 0; ch < this.channelCount; ch++)
      {
        device.Channels[ch] = new MockChannelState();
      }

      this.devices[id] = device;
      Log.Information("MockECLibNative: Connected device {DeviceId} ({Address})", id, address);
      return 0;
    }
  }

  public int BL_Disconnect(int id)
  {
    lock (this.syncRoot)
    {
      this.devices.Remove(id);
      Log.Information("MockECLibNative: Disconnected device {DeviceId}", id);
      return 0;
    }
  }

  public int BL_TestConnection(int id)
  {
    lock (this.syncRoot)
    {
      return this.devices.ContainsKey(id) ? 0 : (int)BL_ERROR.GEN_NOTCONNECTED;
    }
  }

  public int BL_TestCommSpeed(int id, byte channel, out int rcvtSpeed, out int kernelSpeed)
  {
    rcvtSpeed = 9600;
    kernelSpeed = 9600;
    return 0;
  }

  // ---- Device Information ----

  public int BL_GetLibVersion(StringBuilder versionStr, ref uint size)
  {
    const string version = "Mock ECLib 1.0.0";
    versionStr.Clear();
    versionStr.Append(version);
    size = (uint)version.Length;
    return 0;
  }

  public int BL_GetChannelInfos(int id, byte channel, out ChannelInfo channelInfo)
  {
    lock (this.syncRoot)
    {
      channelInfo = default;
      if (!this.TryGetChannel(id, channel, out var chState))
      {
        return (int)BL_ERROR.GEN_CHANNELNOTPLUGGED;
      }

      channelInfo = new ChannelInfo
      {
        Channel = channel,
        BoardVersion = 1,
        BoardSerialNumber = 1000 + channel,
        FirmwareCode = 100,
        FirmwareVersion = 1100,
        XilinxVersion = 437,
        AmpCode = 0,
        NbAmps = 1,
        Lcboard = 0,
        Zboard = 1,
        MUXboard = 0,
        GPRAboard = 0,
        MemSize = 262144,
        MemFilled = 0,
        State = (int)chState.State,
        MaxIRange = (int)I_RANGE.I_RANGE_1A,
        MinIRange = (int)I_RANGE.I_RANGE_100pA,
        MaxBandwidth = (int)BANDWIDTH.BW_9,
        NbOfTechniques = chState.LoadedTechnique != null ? 1 : 0,
      };

      return 0;
    }
  }

  public int BL_GetChannelsPlugged(int id, byte[] channels, byte size)
  {
    lock (this.syncRoot)
    {
      if (!this.devices.TryGetValue(id, out var device))
      {
        return (int)BL_ERROR.GEN_NOTCONNECTED;
      }

      Array.Clear(channels, 0, Math.Min(channels.Length, size));
      foreach (byte ch in device.Channels.Keys)
      {
        if (ch < size)
        {
          channels[ch] = 1;
        }
      }

      return 0;
    }
  }

  public int BL_GetErrorMsg(int errorCode, StringBuilder message, int size)
  {
    message.Clear();
    if (errorCode == 0)
    {
      message.Append("No error");
    }
    else if (Enum.IsDefined(typeof(BL_ERROR), errorCode))
    {
      message.Append(((BL_ERROR)errorCode).ToString());
    }
    else
    {
      message.Append($"Mock error code: {errorCode}");
    }

    return 0;
  }

  public int BL_GetMessage(int id, byte channel, StringBuilder message, ref uint size)
  {
    message.Clear();
    size = 0;
    return 0;
  }

  public int BL_GetOptErr(int id, byte channel, out int optError, out int optPos)
  {
    optError = 0;
    optPos = 0;
    return 0;
  }

  public int BL_GetChannelBoardType(int id, byte channel, out uint boardType)
  {
    boardType = (uint)BOARD_TYPE.ESSENTIAL;
    return 0;
  }

  // ---- Firmware Management ----

  public int BL_LoadFirmware(int id, byte[] channels, int[] results, byte length, bool showGauge, bool forceReload, string? firmwarePath, string? fpgaPath)
  {
    lock (this.syncRoot)
    {
      if (!this.devices.TryGetValue(id, out var device))
      {
        return (int)BL_ERROR.GEN_NOTCONNECTED;
      }

      Array.Clear(results, 0, Math.Min(results.Length, length));
      for (byte ch = 0; ch < Math.Min(length, channels.Length); ch++)
      {
        if (channels[ch] != 0 && device.Channels.TryGetValue(ch, out var chState))
        {
          chState.FirmwareLoaded = true;
        }
      }

      Log.Information("MockECLibNative: Firmware loaded for device {DeviceId}", id);
      return 0;
    }
  }

  // ---- Technique Management ----

  public int BL_LoadTechnique(int id, byte channel, string eccFile, ref EccParams parameters, bool first, bool last, bool display)
  {
    lock (this.syncRoot)
    {
      if (!this.TryGetChannel(id, channel, out var chState))
      {
        return (int)BL_ERROR.GEN_CHANNELNOTPLUGGED;
      }

      chState.LoadedTechnique = eccFile;

      // Reset EIS generation state
      chState.EisProcessPhase = 0;
      chState.EisWarmupEmitted = 0;
      chState.EisSpectrumEmitted = 0;
      chState.EisSpectrumTotal = 0;
      chState.EisInitialFrequency = 0;
      chState.EisFinalFrequency = 0;

      // If technique is GEIS/PEIS, extract frequency parameters from EccParams
      if (IsEisTechnique(eccFile) && parameters.len > 0 && parameters.pParams != IntPtr.Zero)
      {
        int paramSize = System.Runtime.InteropServices.Marshal.SizeOf<EccParam>();
        for (int i = 0; i < parameters.len; i++)
        {
          IntPtr ptr = IntPtr.Add(parameters.pParams, i * paramSize);
          var param = System.Runtime.InteropServices.Marshal.PtrToStructure<EccParam>(ptr);
          string label = System.Text.Encoding.ASCII.GetString(param.ParamStr).TrimEnd('\0');

          if (label == "Initial_frequency")
          {
            chState.EisInitialFrequency = BitConverter.ToSingle(BitConverter.GetBytes(param.ParamVal), 0);
          }
          else if (label == "Final_frequency")
          {
            chState.EisFinalFrequency = BitConverter.ToSingle(BitConverter.GetBytes(param.ParamVal), 0);
          }
          else if (label == "Frequency_number")
          {
            chState.EisSpectrumTotal = param.ParamVal;
          }
        }

        if (chState.EisSpectrumTotal <= 0)
        {
          chState.EisSpectrumTotal = 10;
        }

        if (chState.EisInitialFrequency <= 0)
        {
          chState.EisInitialFrequency = 100000f;
        }

        if (chState.EisFinalFrequency <= 0)
        {
          chState.EisFinalFrequency = 1000f;
        }
      }

      Log.Information("MockECLibNative: Technique '{Technique}' loaded on channel {Channel}", eccFile, channel);
      return 0;
    }
  }

  public int BL_DefineBoolParameter(string label, bool value, int index, out EccParam parameter)
  {
    parameter = CreateEccParam(label, (int)PARAM_TYPE.PARAM_BOOLEAN, value ? 1 : 0, index);
    return 0;
  }

  public int BL_DefineIntParameter(string label, int value, int index, out EccParam parameter)
  {
    parameter = CreateEccParam(label, (int)PARAM_TYPE.PARAM_INT, value, index);
    return 0;
  }

  public int BL_DefineSglParameter(string label, float value, int index, out EccParam parameter)
  {
    parameter = CreateEccParam(label, (int)PARAM_TYPE.PARAM_SINGLE, BitConverter.ToInt32(BitConverter.GetBytes(value), 0), index);
    return 0;
  }

  public int BL_UpdateParameters(int id, byte channel, int index, ref EccParams parameters, string eccFile)
  {
    return 0;
  }

  public int BL_GetTechniqueInfos(int id, byte channel, int index, out TechniqueInfos infos)
  {
    infos = new TechniqueInfos
    {
      Id = 0,
      indx = index,
      nbParams = 0,
      nbSettings = 0,
      Params = IntPtr.Zero,
      HardSettings = IntPtr.Zero,
    };
    return 0;
  }

  public int BL_GetParamInfos(int id, byte channel, int index, out TechniqueInfos infos)
  {
    return this.BL_GetTechniqueInfos(id, channel, index, out infos);
  }

  // ---- Channel Control ----

  public int BL_StartChannel(int id, byte channel)
  {
    lock (this.syncRoot)
    {
      if (!this.TryGetChannel(id, channel, out var chState))
      {
        return (int)BL_ERROR.GEN_CHANNELNOTPLUGGED;
      }

      chState.State = PROG_STATE.RUN;
      chState.StartTime = DateTime.UtcNow;
      chState.DataRowIndex = 0;

      // Reset EIS emission counters so a new run generates fresh data
      chState.EisProcessPhase = 0;
      chState.EisWarmupEmitted = 0;
      chState.EisSpectrumEmitted = 0;

      Log.Information("MockECLibNative: Channel {Channel} started on device {DeviceId}", channel, id);
      return 0;
    }
  }

  public int BL_StartChannels(int id, byte[] channels, int[] results, byte length)
  {
    lock (this.syncRoot)
    {
      if (!this.devices.TryGetValue(id, out var device))
      {
        return (int)BL_ERROR.GEN_NOTCONNECTED;
      }

      Array.Clear(results, 0, Math.Min(results.Length, length));
      for (byte ch = 0; ch < Math.Min(length, channels.Length); ch++)
      {
        if (channels[ch] != 0 && device.Channels.TryGetValue(ch, out var chState))
        {
          chState.State = PROG_STATE.RUN;
          chState.StartTime = DateTime.UtcNow;
          chState.DataRowIndex = 0;
        }
      }

      return 0;
    }
  }

  public int BL_StopChannel(int id, byte channel)
  {
    lock (this.syncRoot)
    {
      if (!this.TryGetChannel(id, channel, out var chState))
      {
        return (int)BL_ERROR.GEN_CHANNELNOTPLUGGED;
      }

      chState.State = PROG_STATE.STOP;
      Log.Information("MockECLibNative: Channel {Channel} stopped on device {DeviceId}", channel, id);
      return 0;
    }
  }

  public int BL_StopChannels(int id, byte[] channels, int[] results, byte length)
  {
    lock (this.syncRoot)
    {
      if (!this.devices.TryGetValue(id, out var device))
      {
        return (int)BL_ERROR.GEN_NOTCONNECTED;
      }

      Array.Clear(results, 0, Math.Min(results.Length, length));
      for (byte ch = 0; ch < Math.Min(length, channels.Length); ch++)
      {
        if (channels[ch] != 0 && device.Channels.TryGetValue(ch, out var chState))
        {
          chState.State = PROG_STATE.STOP;
        }
      }

      return 0;
    }
  }

  // ---- Data Acquisition ----

  public int BL_GetCurrentValues(int id, byte channel, out CurrentValues currentValues)
  {
    lock (this.syncRoot)
    {
      currentValues = default;
      if (!this.TryGetChannel(id, channel, out var chState))
      {
        return (int)BL_ERROR.GEN_CHANNELNOTPLUGGED;
      }

      float elapsed = 0f;
      if (chState.State == PROG_STATE.RUN)
      {
        elapsed = (float)(DateTime.UtcNow - chState.StartTime).TotalSeconds;

        // EIS techniques manage their own stop via BL_GetData; skip timer-based auto-stop
        if (!IsEisTechnique(chState.LoadedTechnique) && elapsed >= this.techniqueRunDurationSeconds)
        {
          chState.State = PROG_STATE.STOP;
          elapsed = this.techniqueRunDurationSeconds;
        }
      }

      currentValues = new CurrentValues
      {
        State = (int)chState.State,
        MemFilled = 0,
        TimeBase = 0.0001f,
        Ewe = 3.0f + 0.5f * MathF.Sin(elapsed * 0.5f),
        EweRangeMin = -10f,
        EweRangeMax = 10f,
        Ece = 0f,
        EceRangeMin = -10f,
        EceRangeMax = 10f,
        Eoverflow = 0,
        I = 0.001f * MathF.Cos(elapsed * 0.3f),
        IRange = (int)I_RANGE.I_RANGE_10mA,
        Ioverflow = 0,
        ElapsedTime = elapsed,
        Freq = 0f,
        Rcomp = 0f,
        Saturation = 0,
        OptErr = 0,
        OptPos = 0,
      };

      return 0;
    }
  }

  public int BL_GetData(int id, byte channel, uint[] dataBuffer, out DataInfo dataInfo, out CurrentValues currentValues)
  {
    lock (this.syncRoot)
    {
      // Fill currentValues via BL_GetCurrentValues
      int cvResult = this.BL_GetCurrentValues(id, channel, out currentValues);
      dataInfo = default;
      if (cvResult != 0)
      {
        return cvResult;
      }

      if (!this.TryGetChannel(id, channel, out var chState))
      {
        return (int)BL_ERROR.GEN_CHANNELNOTPLUGGED;
      }

      // EIS technique: generate proper process 0 / process 1 data
      if (IsEisTechnique(chState.LoadedTechnique) && chState.EisSpectrumTotal > 0)
      {
        return this.GenerateEisData(chState, dataBuffer, out dataInfo, currentValues);
      }

      // Generic technique: simple rows when running
      int rows = chState.State == PROG_STATE.RUN ? 5 : 0;
      int cols = 3;

      dataInfo = new DataInfo
      {
        IRQskipped = 0,
        NbRows = rows,
        NbCols = cols,
        TechniqueIndex = 0,
        TechniqueID = 0,
        ProcessIndex = chState.DataRowIndex,
        loop = 0,
        StartTime = 0,
        MuxPad = 0,
      };

      if (rows > 0)
      {
        chState.DataRowIndex += rows;
      }

      return 0;
    }
  }

  private int GenerateEisData(MockChannelState chState, uint[] dataBuffer, out DataInfo dataInfo, CurrentValues currentValues)
  {
    int techniqueId = GetTechniqueId(chState.LoadedTechnique);

    // Phase 0: Emit a few warmup rows (process 0)
    if (chState.EisProcessPhase == 0)
    {
      const int warmupRows = 3;
      const int warmupCols = 5;

      if (chState.EisWarmupEmitted < warmupRows)
      {
        int rows = Math.Min(warmupRows - chState.EisWarmupEmitted, 3);
        rows = Math.Min(rows, dataBuffer.Length / warmupCols);

        for (int r = 0; r < rows; r++)
        {
          int offset = r * warmupCols;
          float time = chState.EisWarmupEmitted + r + 1;
          dataBuffer[offset + 0] = 0; // padding
          dataBuffer[offset + 1] = 0; // time_hi
          dataBuffer[offset + 2] = (uint)(time / currentValues.TimeBase); // time_lo
          dataBuffer[offset + 3] = FloatToUint(3.0f + 0.01f * r); // Ewe
          dataBuffer[offset + 4] = FloatToUint(0.001f); // Current
        }

        dataInfo = new DataInfo
        {
          NbRows = rows,
          NbCols = warmupCols,
          TechniqueIndex = 0,
          TechniqueID = techniqueId,
          ProcessIndex = 0,
          loop = 0,
        };

        chState.EisWarmupEmitted += rows;
        if (chState.EisWarmupEmitted >= warmupRows)
        {
          chState.EisProcessPhase = 1;
        }

        return 0;
      }

      chState.EisProcessPhase = 1;
    }

    // Phase 1: Emit all remaining spectrum rows (process 1) in a single call.
    // Returning all points at once prevents concurrent BL_GetData callers
    // (e.g. OPC UA polling) from consuming data meant for the GEIS collection loop.
    if (chState.EisProcessPhase == 1)
    {
      int remaining = chState.EisSpectrumTotal - chState.EisSpectrumEmitted;
      if (remaining <= 0)
      {
        chState.EisProcessPhase = 2;
      }
      else
      {
        const int eisCols = 14;
        int rowsPerCall = Math.Min(remaining, dataBuffer.Length / eisCols);

        float logFStart = MathF.Log10(chState.EisInitialFrequency);
        float logFEnd = MathF.Log10(chState.EisFinalFrequency);
        int total = chState.EisSpectrumTotal;

        for (int r = 0; r < rowsPerCall; r++)
        {
          int pointIndex = chState.EisSpectrumEmitted + r;
          float t = total > 1 ? (float)pointIndex / (total - 1) : 0f;
          float frequency = MathF.Pow(10, logFStart + t * (logFEnd - logFStart));

          // Simulated Randles circuit: R_s + 1/(1/R_ct + j*omega*C_dl)
          // R_s = 10 Ohm, R_ct = 100 Ohm, C_dl = 1e-5 F
          float omega = 2f * MathF.PI * frequency;
          float rs = 10f;
          float rct = 100f;
          float cdl = 1e-5f;
          float denom = 1f + omega * omega * rct * rct * cdl * cdl;
          float zReal = rs + rct / denom;
          float zImag = -omega * rct * rct * cdl / denom;
          float zMag = MathF.Sqrt(zReal * zReal + zImag * zImag);
          float phase = MathF.Atan2(zImag, zReal);
          float acAmplitude = 0.01f;
          float potentialAmplitude = acAmplitude * zMag;
          float currentAmplitude = acAmplitude;

          int offset = r * eisCols;
          dataBuffer[offset + 0] = FloatToUint(frequency);              // frequency
          dataBuffer[offset + 1] = FloatToUint(potentialAmplitude);     // potential amplitude
          dataBuffer[offset + 2] = FloatToUint(currentAmplitude);       // current amplitude
          dataBuffer[offset + 3] = FloatToUint(phase);                  // phase Zwe
          dataBuffer[offset + 4] = FloatToUint(3.0f);                   // potential (DC)
          dataBuffer[offset + 5] = FloatToUint(0.001f);                 // current (DC)
          dataBuffer[offset + 6] = FloatToUint(0f);                     // reserved
          dataBuffer[offset + 7] = FloatToUint(potentialAmplitude * 0.1f); // counter potential amplitude
          dataBuffer[offset + 8] = FloatToUint(currentAmplitude * 0.1f);   // counter current amplitude
          dataBuffer[offset + 9] = FloatToUint(phase * 0.1f);           // phase Zce
          dataBuffer[offset + 10] = FloatToUint(0.01f);                 // counter potential
          dataBuffer[offset + 11] = FloatToUint(0f);                    // reserved
          dataBuffer[offset + 12] = FloatToUint(0f);                    // reserved
          dataBuffer[offset + 13] = FloatToUint(pointIndex * 0.5f);     // time
        }

        dataInfo = new DataInfo
        {
          NbRows = rowsPerCall,
          NbCols = eisCols,
          TechniqueIndex = 0,
          TechniqueID = techniqueId,
          ProcessIndex = 1,
          loop = 0,
        };

        chState.EisSpectrumEmitted += rowsPerCall;
        chState.DataRowIndex += rowsPerCall;

        if (chState.EisSpectrumEmitted >= chState.EisSpectrumTotal)
        {
          chState.EisProcessPhase = 2;
        }

        return 0;
      }
    }

    // Phase 2: Done — auto-stop channel and return empty
    if (chState.State == PROG_STATE.RUN)
    {
      chState.State = PROG_STATE.STOP;
    }

    dataInfo = new DataInfo
    {
      NbRows = 0,
      NbCols = 0,
      TechniqueIndex = 0,
      TechniqueID = techniqueId,
      ProcessIndex = 1,
      loop = 0,
    };

    return 0;
  }

  public int BL_ConvertNumericIntoSingle(uint numericValue, out float floatValue)
  {
    floatValue = BitConverter.ToSingle(BitConverter.GetBytes(numericValue), 0);
    return 0;
  }

  public int BL_ConvertChannelNumericIntoSingle(uint numericValue, out float floatValue, uint boardType)
  {
    floatValue = BitConverter.ToSingle(BitConverter.GetBytes(numericValue), 0);
    return 0;
  }

  public int BL_ConvertTimeChannelNumericIntoSeconds(uint[] timeData, out double timeSeconds, float timeBase, uint boardType)
  {
    // Simple mock: combine high/low words into a rough time value
    ulong combined = timeData.Length >= 2 ? ((ulong)timeData[0] << 32) | timeData[1] : 0;
    timeSeconds = combined * (double)timeBase;
    return 0;
  }

  // ---- Hardware Configuration ----

  public int BL_GetHardConf(int id, byte channel, out HardwareConf hardwareConf)
  {
    hardwareConf = new HardwareConf
    {
      Connection = (int)HW_CNX.STANDARD,
      Mode = (int)HW_MODE.GROUNDED,
    };
    return 0;
  }

  public int BL_SetHardConf(int id, byte channel, int connection, int mode)
  {
    return 0;
  }

  // ---- Utility ----

  public string GetShortPath(string longPath) => longPath;

  public string? GetLoadedModulePath(string moduleName) => null;

  // ---- Helpers ----

  private bool TryGetChannel(int deviceId, byte channel, out MockChannelState channelState)
  {
    channelState = null!;
    if (!this.devices.TryGetValue(deviceId, out var device))
    {
      return false;
    }

    return device.Channels.TryGetValue(channel, out channelState!);
  }

  private static EccParam CreateEccParam(string label, int paramType, int paramVal, int paramIndex)
  {
    byte[] labelBytes = new byte[64];
    byte[] src = System.Text.Encoding.ASCII.GetBytes(label);
    Array.Copy(src, labelBytes, Math.Min(src.Length, 64));

    return new EccParam
    {
      ParamStr = labelBytes,
      ParamType = paramType,
      ParamVal = paramVal,
      ParamIndex = paramIndex,
    };
  }

  private static bool IsEisTechnique(string? techniqueFile)
  {
    if (string.IsNullOrEmpty(techniqueFile))
    {
      return false;
    }

    string name = Path.GetFileNameWithoutExtension(techniqueFile).ToLowerInvariant();
    return name is "geis" or "peis";
  }

  private static int GetTechniqueId(string? techniqueFile)
  {
    if (string.IsNullOrEmpty(techniqueFile))
    {
      return 0;
    }

    string name = Path.GetFileNameWithoutExtension(techniqueFile).ToLowerInvariant();
    return name switch
    {
      "geis" => (int)TechniqueId.GEIS,
      "peis" => (int)TechniqueId.PEIS,
      "ocv" => (int)TechniqueId.OCV,
      "cv" => (int)TechniqueId.CV,
      "ca" => (int)TechniqueId.CA,
      "cp" => (int)TechniqueId.CP,
      _ => 0,
    };
  }

  private static uint FloatToUint(float value)
  {
    return BitConverter.ToUInt32(BitConverter.GetBytes(value), 0);
  }
}
