using System.Runtime.InteropServices;

namespace Biologic.Native;

/// <summary>
/// C# type definitions matching EClib DLL structures from kbio_types.py
/// Reference: C:\Program Files (x86)\EC-Lab Development Package\Examples\Python\kbio\kbio_types.py
/// </summary>
/// 
#region Device Information

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct DeviceInfo
{
  public int DeviceCode;
  public int RAMSize;
  public int CPU;
  public int NumberOfChannels;
  public int NumberOfSlots;
  public int FirmwareVersion;
  public int FirmwareDate_yyyy;
  public int FirmwareDate_mm;
  public int FirmwareDate_dd;
  public int HTdisplayOn;
  public int NbOfConnectedPC;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ChannelInfo
{
  public int Channel;                // Channel (0..15)
  public int BoardVersion;           // Board version
  public int BoardSerialNumber;      // Board serial number
  public int FirmwareCode;           // Identifier of the firmware loaded
  public int FirmwareVersion;        // Firmware version
  public int XilinxVersion;          // Xilinx version
  public int AmpCode;                // Amplifier code
  public int NbAmps;                 // Number of amplifiers (0..16)
  public int Lcboard;                // Low current presence
  public int Zboard;                 // Impedance capabilities
  public int MUXboard;               // MEA mux
  public int GPRAboard;              // Analog ramp
  public int MemSize;                // Memory size (in bytes)
  public int MemFilled;              // Memory filled (in bytes)
  public int State;                  // Channel state: run/stop/pause
  public int MaxIRange;              // Maximum I range allowed
  public int MinIRange;              // Minimum I range allowed
  public int MaxBandwidth;           // Maximum bandwidth allowed
  public int NbOfTechniques;         // Number of techniques loaded
}

#endregion

#region Hardware Configuration

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct HardwareConf
{
  public int Connection;  // Electrode connection
  public int Mode;        // Channel mode
}

public enum HW_CNX
{
  STANDARD = 0,        // Standard connection
  CE_TO_GND = 1,       // CE to ground connection
  WE_TO_GND = 2,       // WE to ground connection
  HIGH_VOLTAGE = 3     // 48V connection
}

public enum HW_MODE
{
  GROUNDED = 0,   // Grounded mode
  FLOATING = 1    // Floating mode
}

#endregion

#region Current Values and Data

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct CurrentValues
{
  public int State;
  public int MemFilled;
  public float TimeBase;
  public float Ewe;
  public float EweRangeMin;
  public float EweRangeMax;
  public float Ece;
  public float EceRangeMin;
  public float EceRangeMax;
  public int Eoverflow;
  public float I;
  public int IRange;
  public int Ioverflow;
  public float ElapsedTime;
  public float Freq;
  public float Rcomp;
  public int Saturation;
  public int OptErr;
  public int OptPos;
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct DataInfo
{
  public int IRQskipped;
  public int NbRows;
  public int NbCols;
  public int TechniqueIndex;
  public int TechniqueID;
  public int ProcessIndex;
  public int loop;
  public double StartTime;
  public int MuxPad;
}

#endregion

#region Technique Parameters

public enum PARAM_TYPE
{
  PARAM_INT = 0,
  PARAM_BOOLEAN = 1,
  PARAM_SINGLE = 2
}

// CRITICAL: Structure layout MUST match C++ definition exactly!
// From BLStructs.h:
//   typedef struct {
//       char ParamStr[64];  // 64 bytes
//       int ParamType;      // 4 bytes
//       int ParamVal;       // 4 bytes
//       int ParamIndex;     // 4 bytes
//   } TEccParam_t;
//
// Total size: 76 bytes (64 + 4 + 4 + 4)
// Pack = 4 for natural alignment (matching C++ default)
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct EccParam
{
  [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
  public byte[] ParamStr;
  public int ParamType;
  public int ParamVal;    // Changed from uint to int to match C++ definition
  public int ParamIndex;
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct EccParams
{
  public int len;
  public IntPtr pParams;  // Pointer to EccParam array
}

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct TechniqueInfos
{
  public int Id;
  public int indx;
  public int nbParams;
  public int nbSettings;
  public IntPtr Params;
  public IntPtr HardSettings;
}

#endregion

#region Enumerations

public enum DEVICE
{
  VMP = 0,
  VMP2 = 1,
  MPG = 2,
  BISTAT = 3,
  MCS_200 = 4,
  VMP3 = 5,
  VSP = 6,
  HCP803 = 7,
  EPP400 = 8,
  EPP4000 = 9,
  BISTAT2 = 10,
  FCT150S = 11,
  VMP300 = 12,
  SP50 = 13,
  SP150 = 14,
  FCT50S = 15,
  SP300 = 16,
  CLB500 = 17,
  HCP1005 = 18,
  CLB2000 = 19,
  VSP300 = 20,
  SP200 = 21,
  MPG2 = 22,
  SP100 = 23,
  MOSLED = 24,
  KINEXXX = 25,
  BCS815 = 26,
  SP240 = 27,
  MPG205 = 28,
  MPG210 = 29,
  MPG220 = 30,
  MPG240 = 31,
  BP300 = 32,
  VMP3E = 33,
  VSP3E = 34,
  SP50E = 35,
  SP150E = 36,
  UNKNOWN = 255
}

public enum BOARD_TYPE
{
  UNKNOWN = 0,
  ESSENTIAL = 1,
  PREMIUM = 2,
  DIGICORE = 3
}

public enum PROG_STATE
{
  STOP = 0,   // Channel is stopped
  RUN = 1,    // Channel is running
  PAUSE = 2,  // Channel is paused
  SYNC = 3    // Grouped channels synchronization (stack)
}

public enum I_RANGE
{
  I_RANGE_KEEP = -1,      // Keep previous
  I_RANGE_100pA = 0,      // 100 pA
  I_RANGE_1nA = 1,        // 1 nA VMP3
  I_RANGE_10nA = 2,       // 10 nA VMP3
  I_RANGE_100nA = 3,      // 100 nA VMP3
  I_RANGE_1uA = 4,        // 1 μA VMP3
  I_RANGE_10uA = 5,       // 10 μA VMP3
  I_RANGE_100uA = 6,      // 100 μA VMP3
  I_RANGE_1mA = 7,        // 1 mA VMP3
  I_RANGE_10mA = 8,       // 10 mA VMP3
  I_RANGE_100mA = 9,      // 100 mA VMP3
  I_RANGE_1A = 10,        // 1 A VMP3
  I_RANGE_BOOSTER = 11,   // Booster VMP3
  I_RANGE_AUTO = 12       // Auto range VMP3
}

public enum E_RANGE
{
  E_RANGE_2_5V = 0,   // ±2.5V
  E_RANGE_5V = 1,     // ±5V
  E_RANGE_10V = 2,    // ±10V
  E_RANGE_AUTO = 3    // auto
}

public enum BANDWIDTH
{
  BW_1 = 1,
  BW_2 = 2,
  BW_3 = 3,
  BW_4 = 4,
  BW_5 = 5,
  BW_6 = 6,
  BW_7 = 7,
  BW_8 = 8,
  BW_9 = 9
}

#endregion

#region Error Codes

public enum BL_ERROR
{
  NOERROR = 0,
  GEN_NOTCONNECTED = -1,
  GEN_CONNECTIONINPROGRESS = -2,
  GEN_CHANNELNOTPLUGGED = -3,
  GEN_INVALIDPARAMETERS = -4,
  GEN_FILENOTEXISTS = -5,
  GEN_FUNCTIONFAILED = -6,
  GEN_NOCHANNELSELECTED = -7,
  GEN_INVALIDCONF = -8,
  GEN_ECLAB_LOADED = -9,
  GEN_LIBNOTCORRECTLYLOADED = -10,
  GEN_USBLIBRARYERROR = -11,
  GEN_FUNCTIONINPROGRESS = -12,
  GEN_CHANNEL_RUNNING = -13,
  GEN_DEVICE_NOTALLOWED = -14,
  GEN_UPDATEPARAMETERS = -15,
  INSTR_VMEERROR = -101,
  INSTR_TOOMANYDATA = -102,
  INSTR_RESPNOTPOSSIBLE = -103,
  INSTR_RESPERROR = -104,
  INSTR_MSGSIZEERROR = -105,
  COMM_COMMFAILED = -200,
  COMM_CONNECTIONFAILED = -201,
  COMM_WAITINGACK = -202,
  COMM_INVALIDIPADDRESS = -203,
  COMM_ALLOCMEMFAILED = -204,
  COMM_LOADFIRMWAREFAILED = -205,
  COMM_INCOMPATIBLESERVER = -206,
  COMM_MAXCONNREACHED = -207,
  FIRM_FIRMFILENOTEXISTS = -300,
  FIRM_FIRMFILEACCESSFAILED = -301,
  FIRM_FIRMINVALIDFILE = -302,
  FIRM_FIRMLOADINGFAILED = -303,
  FIRM_XILFILENOTEXISTS = -304,
  FIRM_XILFILEACCESSFAILED = -305,
  FIRM_XILINVALIDFILE = -306,
  FIRM_XILLOADINGFAILED = -307,
  FIRM_FIRMWARENOTLOADED = -308,
  FIRM_FIRMWAREINCOMPATIBLE = -309,
  TECH_ECCFILENOTEXISTS = -400,
  TECH_INCOMPATIBLEECC = -401,
  TECH_ECCFILECORRUPTED = -402,
  TECH_LOADTECHNIQUEFAILED = -403,
  TECH_DATACORRUPTED = -404,
  TECH_MEMFULL = -405
}

#endregion
