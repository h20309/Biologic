namespace Biologic.Techniques;

/// <summary>
/// Base class for all technique parameters
/// </summary>
public abstract class TechniqueParameterBase
{
  /// <summary>
  /// Technique ID
  /// </summary>
  public abstract Native.TechniqueId TechniqueId { get; }

  /// <summary>
  /// Get technique file name (.ecc file)
  /// </summary>
  public abstract string GetTechniqueFileName();

  /// <summary>
  /// Build parameter dictionary for LoadTechniqueSequenceItem
  /// </summary>
  public abstract Dictionary<string, object> BuildParameters();

  /// <summary>
  /// Validate parameters
  /// </summary>
  public virtual bool Validate(out string errorMessage)
  {
    errorMessage = string.Empty;
    return true;
  }
}

/// <summary>
/// Open Circuit Voltage (OCV) parameters
/// Reference: EC-Lab Development Package PDF Section 7.1
/// </summary>
public class OcvParameters : TechniqueParameterBase
{
  public override Native.TechniqueId TechniqueId => Native.TechniqueId.OCV;

  /// <summary>Rest time (s)</summary>
  public float RestTime_s { get; set; } = 10.0f;

  /// <summary>Record every dE (V)</summary>
  public float RecordEvery_dE { get; set; } = 0.01f;

  /// <summary>Record every dt (s)</summary>
  public float RecordEvery_dt { get; set; } = 1.0f;

  public override string GetTechniqueFileName() => "ocv.ecc";

  public override Dictionary<string, object> BuildParameters()
  {
    return new Dictionary<string, object>
    {
      ["Rest_time_T"] = RestTime_s,
      ["Record_every_dE"] = RecordEvery_dE,
      ["Record_every_dt"] = RecordEvery_dt
    };
  }

  public override bool Validate(out string errorMessage)
  {
    if (RestTime_s <= 0)
    {
      errorMessage = "Rest time must be positive";
      return false;
    }

    errorMessage = string.Empty;
    return true;
  }
}

/// <summary>
/// Chrono-Potentiometry (CP) parameters
/// Reference: EC-Lab Development Package PDF Section 7.3
/// </summary>
public class CpParameters : TechniqueParameterBase
{
  public override Native.TechniqueId TechniqueId => Native.TechniqueId.CP;

  /// <summary>Voltage step (V)</summary>
  public float VoltageStep_V { get; set; } = 0.0f;

  /// <summary>Duration (s)</summary>
  public float Duration_s { get; set; } = 10.0f;

  /// <summary>Record every dI (A)</summary>
  public float RecordEvery_dI { get; set; } = 0.001f;

  /// <summary>Record every dt (s)</summary>
  public float RecordEvery_dt { get; set; } = 0.1f;

  /// <summary>Number of cycles</summary>
  public int NCycles { get; set; } = 1;

  public override string GetTechniqueFileName() => "cp.ecc";

  public override Dictionary<string, object> BuildParameters()
  {
    return new Dictionary<string, object>
    {
      ["Voltage_step"] = VoltageStep_V,
      ["Duration_step"] = Duration_s,
      ["Record_every_dI"] = RecordEvery_dI,
      ["Record_every_dt"] = RecordEvery_dt,
      ["N_Cycles"] = NCycles
    };
  }

  public override bool Validate(out string errorMessage)
  {
    if (Duration_s <= 0)
    {
      errorMessage = "Duration must be positive";
      return false;
    }

    if (NCycles < 1)
    {
      errorMessage = "Number of cycles must be at least 1";
      return false;
    }

    errorMessage = string.Empty;
    return true;
  }
}

/// <summary>
/// Chrono-Amperometry (CA) parameters
/// Reference: EC-Lab Development Package PDF Section 7.2
/// </summary>
public class CaParameters : TechniqueParameterBase
{
  public override Native.TechniqueId TechniqueId => Native.TechniqueId.CA;

  /// <summary>Current step (A)</summary>
  public float CurrentStep_A { get; set; } = 0.001f;

  /// <summary>Duration (s)</summary>
  public float Duration_s { get; set; } = 10.0f;

  /// <summary>Record every dE (V)</summary>
  public float RecordEvery_dE { get; set; } = 0.01f;

  /// <summary>Record every dt (s)</summary>
  public float RecordEvery_dt { get; set; } = 0.1f;

  /// <summary>Number of cycles</summary>
  public int NCycles { get; set; } = 1;

  public override string GetTechniqueFileName() => "ca.ecc";

  public override Dictionary<string, object> BuildParameters()
  {
    return new Dictionary<string, object>
    {
      ["Current_step"] = CurrentStep_A,
      ["Duration_step"] = Duration_s,
      ["Record_every_dE"] = RecordEvery_dE,
      ["Record_every_dt"] = RecordEvery_dt,
      ["N_Cycles"] = NCycles
    };
  }

  public override bool Validate(out string errorMessage)
  {
    if (Duration_s <= 0)
    {
      errorMessage = "Duration must be positive";
      return false;
    }

    if (NCycles < 1)
    {
      errorMessage = "Number of cycles must be at least 1";
      return false;
    }

    errorMessage = string.Empty;
    return true;
  }
}

/// <summary>
/// Cyclic Voltammetry (CV) parameters
/// Reference: EC-Lab Development Package PDF Section 7.4
/// </summary>
public class CvParameters : TechniqueParameterBase
{
  public override Native.TechniqueId TechniqueId => Native.TechniqueId.CV;

  /// <summary>Start voltage (V vs Ref)</summary>
  public float StartVoltage_V { get; set; } = 0.0f;

  /// <summary>Vertex 1 voltage (V vs Ref)</summary>
  public float Vertex1_V { get; set; } = 1.0f;

  /// <summary>Vertex 2 voltage (V vs Ref)</summary>
  public float Vertex2_V { get; set; } = -1.0f;

  /// <summary>Scan rate (V/s)</summary>
  public float ScanRate_V_s { get; set; } = 0.1f;

  /// <summary>Number of cycles</summary>
  public int NCycles { get; set; } = 1;

  /// <summary>Record every dE (V)</summary>
  public float RecordEvery_dE { get; set; } = 0.01f;

  /// <summary>Average over points</summary>
  public int AverageOverPoints { get; set; } = 1;

  public override string GetTechniqueFileName() => "cv.ecc";

  public override Dictionary<string, object> BuildParameters()
  {
    return new Dictionary<string, object>
    {
      ["vs_initial"] = StartVoltage_V,
      ["Vertex_1_E"] = Vertex1_V,
      ["Vertex_2_E"] = Vertex2_V,
      ["Scan_Rate"] = ScanRate_V_s,
      ["N_Cycles"] = NCycles,
      ["Record_every_dE"] = RecordEvery_dE,
      ["Average_over_dE"] = AverageOverPoints
    };
  }

  public override bool Validate(out string errorMessage)
  {
    if (ScanRate_V_s <= 0)
    {
      errorMessage = "Scan rate must be positive";
      return false;
    }

    if (NCycles < 1)
    {
      errorMessage = "Number of cycles must be at least 1";
      return false;
    }

    if (Math.Abs(Vertex1_V - Vertex2_V) < 0.001f)
    {
      errorMessage = "Vertex voltages must be different";
      return false;
    }

    errorMessage = string.Empty;
    return true;
  }
}
