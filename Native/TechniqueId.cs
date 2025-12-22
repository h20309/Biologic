namespace Biologic.Native;

/// <summary>
/// Technique identifiers matching BioLogic TTechniqueIdentifier_e enum
/// Reference: BLStructs.h - TTechniqueIdentifier_e
/// </summary>
public enum TechniqueId
{
  /// <summary>None</summary>
  None = 0,

  // Basic electrochemical techniques (100-107)
  /// <summary>Open Circuit Voltage (Rest)</summary>
  OCV = 100,

  /// <summary>Chrono-Amperometry</summary>
  CA = 101,

  /// <summary>Chrono-Potentiometry</summary>
  CP = 102,

  /// <summary>Cyclic Voltammetry</summary>
  CV = 103,

  /// <summary>Potentio Electrochemical Impedance Spectroscopy</summary>
  PEIS = 104,

  /// <summary>Potentiostatic Pulse (unused)</summary>
  POTPULSE = 105,

  /// <summary>Galvanostatic Pulse (unused)</summary>
  GALPULSE = 106,

  /// <summary>Galvano Electrochemical Impedance Spectroscopy</summary>
  GEIS = 107,

  // Stack techniques (108-120)
  /// <summary>Potentio EIS on stack (slave)</summary>
  STACKPEIS_SLAVE = 108,

  /// <summary>Potentio EIS on stack</summary>
  STACKPEIS = 109,

  /// <summary>Constant Power</summary>
  CPOWER = 110,

  /// <summary>Constant Load</summary>
  CLOAD = 111,

  /// <summary>Fuel Cell Test (unused)</summary>
  FCT = 112,

  /// <summary>Staircase Potentio EIS</summary>
  SPEIS = 113,

  /// <summary>Staircase Galvano EIS</summary>
  SGEIS = 114,

  /// <summary>Potentio dynamic on stack</summary>
  STACKPDYN = 115,

  /// <summary>Potentio dynamic on stack (slave)</summary>
  STACKPDYN_SLAVE = 116,

  /// <summary>Galvano dynamic on stack</summary>
  STACKGDYN = 117,

  /// <summary>Galvano EIS on stack (slave)</summary>
  STACKGEIS_SLAVE = 118,

  /// <summary>Galvano EIS on stack</summary>
  STACKGEIS = 119,

  /// <summary>Galvano dynamic on stack (slave)</summary>
  STACKGDYN_SLAVE = 120,

  // Dynamic techniques (121-125)
  /// <summary>CPO (unused)</summary>
  CPO = 121,

  /// <summary>CGA (unused)</summary>
  CGA = 122,

  /// <summary>COKINE (unused)</summary>
  COKINE = 123,

  /// <summary>Potentio Dynamic</summary>
  PDYN = 124,

  /// <summary>Galvano Dynamic</summary>
  GDYN = 125,

  // Voltammetry techniques (126-132)
  /// <summary>Cyclic Voltammetry Advanced</summary>
  CVA = 126,

  /// <summary>Differential Pulse Voltammetry</summary>
  DPV = 127,

  /// <summary>Square Wave Voltammetry</summary>
  SWV = 128,

  /// <summary>Normal Pulse Voltammetry</summary>
  NPV = 129,

  /// <summary>Reverse Normal Pulse Voltammetry</summary>
  RNPV = 130,

  /// <summary>Differential Normal Pulse Voltammetry</summary>
  DNPV = 131,

  /// <summary>Differential Pulse Amperometry</summary>
  DPA = 132,

  // Corrosion techniques (133-142)
  /// <summary>Ecorr vs. time</summary>
  EVT = 133,

  /// <summary>Linear Polarization</summary>
  LP = 134,

  /// <summary>Generalized Corrosion</summary>
  GC = 135,

  /// <summary>Cyclic Potentiodynamic Polarization</summary>
  CPP = 136,

  /// <summary>Potentiodynamic Pitting</summary>
  PDP = 137,

  /// <summary>Potentiostatic Pitting</summary>
  PSP = 138,

  /// <summary>Zero Resistance Ammeter</summary>
  ZRA = 139,

  /// <summary>Manual IR</summary>
  MIR = 140,

  /// <summary>IR Determination with Potentiostatic Impedance</summary>
  PZIR = 141,

  /// <summary>IR Determination with Galvanostatic Impedance</summary>
  GZIR = 142,

  // Control techniques (150-158)
  /// <summary>Loop (for linked techniques)</summary>
  LOOP = 150,

  /// <summary>Trigger Out</summary>
  TO = 151,

  /// <summary>Trigger In</summary>
  TI = 152,

  /// <summary>Trigger Set</summary>
  TOS = 153,

  /// <summary>Chrono-Potentiometry with limits</summary>
  CPLIMIT = 155,

  /// <summary>Galvano Dynamic with limits</summary>
  GDYNLIMIT = 156,

  /// <summary>Chrono-Amperometry with limits</summary>
  CALIMIT = 157,

  /// <summary>Potentio Dynamic with limits</summary>
  PDYNLIMIT = 158,

  /// <summary>Large Amplitude Sinusoidal Voltammetry</summary>
  LASV = 159,

  // Advanced techniques (160-172)
  /// <summary>Mux Loop</summary>
  MUXLOOP = 160,

  /// <summary>CV + CA</summary>
  CVCA = 161,

  /// <summary>CV + CA (slave)</summary>
  CVCA_SLAVE = 162,

  /// <summary>CP + CA</summary>
  CPCA = 163,

  /// <summary>CP + CA (slave)</summary>
  CPCA_SLAVE = 164,

  /// <summary>CA + CA</summary>
  CACA = 165,

  /// <summary>CA + CA (slave)</summary>
  CACA_SLAVE = 166,

  /// <summary>Modular Pulse</summary>
  MP = 167,

  /// <summary>Constant Amplitude Sinusoidal Micro Galvano Polarization</summary>
  CASG = 169,

  /// <summary>Constant Amplitude Sinusoidal Micro Potentio Polarization</summary>
  CASP = 170,

  /// <summary>Variable Amplitude Sinusoidal Potentio</summary>
  VASP = 171,

  /// <summary>UCV Analog</summary>
  UCVANALOG = 172,

  // Special techniques
  /// <summary>Universal Panel</summary>
  UNIPANEL = 200,

  // Relaxation techniques (500-502)
  /// <summary>OCV Relaxation</summary>
  OCVR = 500,

  /// <summary>CA Relaxation</summary>
  CAR = 501,

  /// <summary>CP Relaxation</summary>
  CPR = 502,

  // Optical techniques (1000-1008)
  /// <summary>Absorbance</summary>
  ABS = 1000,

  /// <summary>Fluorescence</summary>
  FLUO = 1001,

  /// <summary>Ratiometric Absorbance</summary>
  RABS = 1002,

  /// <summary>Ratiometric Fluorescence</summary>
  RFLUO = 1003,

  /// <summary>Differential Absorbance</summary>
  RDABS = 1004,

  /// <summary>Delta Absorbance</summary>
  DABS = 1005,

  /// <summary>Absorbance + Fluorescence</summary>
  ABSFLUO = 1006,

  /// <summary>Ratiometric Absorbance (alternate)</summary>
  RAFABS = 1007,

  /// <summary>Ratiometric Fluorescence (alternate)</summary>
  RAFFLUO = 1008
}

/// <summary>
/// Extension methods for TechniqueId enum
/// </summary>
public static class TechniqueIdExtensions
{
  /// <summary>
  /// Get technique name as string
  /// </summary>
  public static string GetName(this TechniqueId techniqueId)
  {
    return techniqueId switch
    {
      TechniqueId.OCV => "Open Circuit Voltage",
      TechniqueId.CA => "Chrono-Amperometry",
      TechniqueId.CP => "Chrono-Potentiometry",
      TechniqueId.CV => "Cyclic Voltammetry",
      TechniqueId.PEIS => "Potentio EIS",
      TechniqueId.GEIS => "Galvano EIS",
      TechniqueId.CPOWER => "Constant Power",
      TechniqueId.CLOAD => "Constant Load",
      TechniqueId.SPEIS => "Staircase Potentio EIS",
      TechniqueId.SGEIS => "Staircase Galvano EIS",
      TechniqueId.PDYN => "Potentio Dynamic",
      TechniqueId.GDYN => "Galvano Dynamic",
      TechniqueId.CVA => "Cyclic Voltammetry Advanced",
      TechniqueId.DPV => "Differential Pulse Voltammetry",
      TechniqueId.SWV => "Square Wave Voltammetry",
      TechniqueId.NPV => "Normal Pulse Voltammetry",
      TechniqueId.ZRA => "Zero Resistance Ammeter",
      _ => techniqueId.ToString()
    };
  }

  /// <summary>
  /// Check if technique is an impedance spectroscopy technique
  /// </summary>
  public static bool IsEIS(this TechniqueId techniqueId)
  {
    return techniqueId switch
    {
      TechniqueId.PEIS or
      TechniqueId.GEIS or
      TechniqueId.SPEIS or
      TechniqueId.SGEIS or
      TechniqueId.STACKPEIS or
      TechniqueId.STACKGEIS or
      TechniqueId.PZIR or
      TechniqueId.GZIR => true,
      _ => false
    };
  }

  /// <summary>
  /// Check if technique is a voltammetry technique
  /// </summary>
  public static bool IsVoltammetry(this TechniqueId techniqueId)
  {
    return techniqueId switch
    {
      TechniqueId.CV or
      TechniqueId.CVA or
      TechniqueId.DPV or
      TechniqueId.SWV or
      TechniqueId.NPV or
      TechniqueId.RNPV or
      TechniqueId.DNPV or
      TechniqueId.LASV => true,
      _ => false
    };
  }

  /// <summary>
  /// Check if technique is a corrosion technique
  /// </summary>
  public static bool IsCorrosion(this TechniqueId techniqueId)
  {
    return techniqueId switch
    {
      TechniqueId.EVT or
      TechniqueId.LP or
      TechniqueId.GC or
      TechniqueId.CPP or
      TechniqueId.PDP or
      TechniqueId.PSP or
      TechniqueId.ZRA => true,
      _ => false
    };
  }
}
