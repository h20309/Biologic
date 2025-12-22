namespace Biologic.Techniques;

/// <summary>
/// Potentio Electrochemical Impedance Spectroscopy (PEIS) parameters
/// Reference: EC-Lab Development Package PDF Section 7.5
/// </summary>
public class PeisParameters : TechniqueParameterBase
{
  public override Native.TechniqueId TechniqueId => Native.TechniqueId.PEIS;

  /// <summary>Initial voltage (V vs Ref)</summary>
  public float InitialVoltage_V { get; set; } = 0.0f;

  /// <summary>Final voltage (V vs Ref)</summary>
  public float FinalVoltage_V { get; set; } = 0.0f;

  /// <summary>Duration (s)</summary>
  public float Duration_s { get; set; } = 0.0f;

  /// <summary>AC voltage amplitude (V)</summary>
  public float VAC_V { get; set; } = 0.01f;

  /// <summary>Initial frequency (Hz)</summary>
  public float InitialFreq_Hz { get; set; } = 100000.0f;

  /// <summary>Final frequency (Hz)</summary>
  public float FinalFreq_Hz { get; set; } = 0.1f;

  /// <summary>Points per decade</summary>
  public int PointsPerDecade { get; set; } = 10;

  /// <summary>Wait for steady state</summary>
  public bool WaitForSteadyState { get; set; } = true;

  public override string GetTechniqueFileName() => "peis.ecc";

  public override Dictionary<string, object> BuildParameters()
  {
    return new Dictionary<string, object>
    {
      ["vs_initial"] = InitialVoltage_V,
      ["vs_final"] = FinalVoltage_V,
      ["Duration"] = Duration_s,
      ["Va"] = VAC_V,
      ["freq_init"] = InitialFreq_Hz,
      ["freq_final"] = FinalFreq_Hz,
      ["Points_per_decade"] = PointsPerDecade,
      ["Wait_for_steady"] = WaitForSteadyState
    };
  }

  public override bool Validate(out string errorMessage)
  {
    if (VAC_V <= 0 || VAC_V > 1.0f)
    {
      errorMessage = "AC voltage amplitude must be between 0 and 1 V";
      return false;
    }

    if (InitialFreq_Hz <= FinalFreq_Hz)
    {
      errorMessage = "Initial frequency must be greater than final frequency";
      return false;
    }

    if (PointsPerDecade < 1)
    {
      errorMessage = "Points per decade must be at least 1";
      return false;
    }

    errorMessage = string.Empty;
    return true;
  }
}

/// <summary>
/// Galvano Electrochemical Impedance Spectroscopy (GEIS) parameters
/// Reference: EC-Lab Development Package PDF Section 7.8
/// </summary>
public class GeisParameters : TechniqueParameterBase
{
  public override Native.TechniqueId TechniqueId => Native.TechniqueId.GEIS;

  /// <summary>Initial current (A)</summary>
  public float InitialCurrent_A { get; set; } = 0.0f;

  /// <summary>Final current (A)</summary>
  public float FinalCurrent_A { get; set; } = 0.0f;

  /// <summary>Duration (s)</summary>
  public float Duration_s { get; set; } = 0.0f;

  /// <summary>AC current amplitude (A)</summary>
  public float IAC_A { get; set; } = 0.0001f;

  /// <summary>Initial frequency (Hz)</summary>
  public float InitialFreq_Hz { get; set; } = 100000.0f;

  /// <summary>Final frequency (Hz)</summary>
  public float FinalFreq_Hz { get; set; } = 0.1f;

  /// <summary>Points per decade</summary>
  public int PointsPerDecade { get; set; } = 10;

  /// <summary>Wait for steady state</summary>
  public bool WaitForSteadyState { get; set; } = true;

  public override string GetTechniqueFileName() => "geis.ecc";

  public override Dictionary<string, object> BuildParameters()
  {
    return new Dictionary<string, object>
    {
      ["Is_initial"] = InitialCurrent_A,
      ["Is_final"] = FinalCurrent_A,
      ["Duration"] = Duration_s,
      ["Ia"] = IAC_A,
      ["freq_init"] = InitialFreq_Hz,
      ["freq_final"] = FinalFreq_Hz,
      ["Points_per_decade"] = PointsPerDecade,
      ["Wait_for_steady"] = WaitForSteadyState
    };
  }

  public override bool Validate(out string errorMessage)
  {
    if (IAC_A <= 0)
    {
      errorMessage = "AC current amplitude must be positive";
      return false;
    }

    if (InitialFreq_Hz <= FinalFreq_Hz)
    {
      errorMessage = "Initial frequency must be greater than final frequency";
      return false;
    }

    if (PointsPerDecade < 1)
    {
      errorMessage = "Points per decade must be at least 1";
      return false;
    }

    errorMessage = string.Empty;
    return true;
  }
}

/// <summary>
/// Zero Resistance Ammeter (ZRA) parameters
/// Reference: EC-Lab Development Package PDF Section 7.39
/// </summary>
public class ZraParameters : TechniqueParameterBase
{
  public override Native.TechniqueId TechniqueId => Native.TechniqueId.ZRA;

  /// <summary>Duration (s)</summary>
  public float Duration_s { get; set; } = 10.0f;

  /// <summary>Record every dE (V)</summary>
  public float RecordEvery_dE { get; set; } = 0.001f;

  /// <summary>Record every dI (A)</summary>
  public float RecordEvery_dI { get; set; } = 0.0001f;

  /// <summary>Record every dt (s)</summary>
  public float RecordEvery_dt { get; set; } = 0.1f;

  /// <summary>Current range (see TIntensityRange_e)</summary>
  public int IRange { get; set; } = 4; // 1 uA

  public override string GetTechniqueFileName() => "zra.ecc";

  public override Dictionary<string, object> BuildParameters()
  {
    return new Dictionary<string, object>
    {
      ["Duration"] = Duration_s,
      ["Record_every_dE"] = RecordEvery_dE,
      ["Record_every_dI"] = RecordEvery_dI,
      ["Record_every_dt"] = RecordEvery_dt,
      ["I_Range"] = IRange
    };
  }

  public override bool Validate(out string errorMessage)
  {
    if (Duration_s <= 0)
    {
      errorMessage = "Duration must be positive";
      return false;
    }

    if (IRange < 0 || IRange > 14)
    {
      errorMessage = "Invalid current range";
      return false;
    }

    errorMessage = string.Empty;
    return true;
  }
}

/// <summary>
/// Constant Power (CPOWER) parameters
/// Reference: EC-Lab Development Package PDF Section 7.11
/// </summary>
public class CpowerParameters : TechniqueParameterBase
{
  public override Native.TechniqueId TechniqueId => Native.TechniqueId.CPOWER;

  /// <summary>Power (W)</summary>
  public float Power_W { get; set; } = 1.0f;

  /// <summary>Duration (s)</summary>
  public float Duration_s { get; set; } = 10.0f;

  /// <summary>Record every dE (V)</summary>
  public float RecordEvery_dE { get; set; } = 0.01f;

  /// <summary>Record every dI (A)</summary>
  public float RecordEvery_dI { get; set; } = 0.001f;

  /// <summary>Record every dt (s)</summary>
  public float RecordEvery_dt { get; set; } = 0.1f;

  public override string GetTechniqueFileName() => "cpower.ecc";

  public override Dictionary<string, object> BuildParameters()
  {
    return new Dictionary<string, object>
    {
      ["Power"] = Power_W,
      ["Duration"] = Duration_s,
      ["Record_every_dE"] = RecordEvery_dE,
      ["Record_every_dI"] = RecordEvery_dI,
      ["Record_every_dt"] = RecordEvery_dt
    };
  }

  public override bool Validate(out string errorMessage)
  {
    if (Duration_s <= 0)
    {
      errorMessage = "Duration must be positive";
      return false;
    }

    errorMessage = string.Empty;
    return true;
  }
}

/// <summary>
/// Constant Load (CLOAD) parameters
/// Reference: EC-Lab Development Package PDF Section 7.12
/// </summary>
public class CloadParameters : TechniqueParameterBase
{
  public override Native.TechniqueId TechniqueId => Native.TechniqueId.CLOAD;

  /// <summary>Load resistance (Ohm)</summary>
  public float Load_Ohm { get; set; } = 1.0f;

  /// <summary>Duration (s)</summary>
  public float Duration_s { get; set; } = 10.0f;

  /// <summary>Record every dE (V)</summary>
  public float RecordEvery_dE { get; set; } = 0.01f;

  /// <summary>Record every dI (A)</summary>
  public float RecordEvery_dI { get; set; } = 0.001f;

  /// <summary>Record every dt (s)</summary>
  public float RecordEvery_dt { get; set; } = 0.1f;

  public override string GetTechniqueFileName() => "cload.ecc";

  public override Dictionary<string, object> BuildParameters()
  {
    return new Dictionary<string, object>
    {
      ["Load"] = Load_Ohm,
      ["Duration"] = Duration_s,
      ["Record_every_dE"] = RecordEvery_dE,
      ["Record_every_dI"] = RecordEvery_dI,
      ["Record_every_dt"] = RecordEvery_dt
    };
  }

  public override bool Validate(out string errorMessage)
  {
    if (Load_Ohm <= 0)
    {
      errorMessage = "Load resistance must be positive";
      return false;
    }

    if (Duration_s <= 0)
    {
      errorMessage = "Duration must be positive";
      return false;
    }

    errorMessage = string.Empty;
    return true;
  }
}
