namespace Biologic;

/// <summary>
/// Method parameters for Biologic OPC UA Methods
/// Used for dynamic technique execution via OPC UA
/// </summary>
public class MethodParameters
{
  /// <summary>
  /// Parameters for running a CV scan
  /// </summary>
  /// <param name="ChannelIndex">Zero-based channel index (0-15)</param>
  /// <param name="StartVoltage_V">Start voltage in volts</param>
  /// <param name="Vertex1_V">First vertex voltage in volts</param>
  /// <param name="Vertex2_V">Second vertex voltage in volts</param>
  /// <param name="ScanRate_V_s">Scan rate in V/s</param>
  /// <param name="NCycles">Number of cycles</param>
  /// <param name="OutputFile">Output CSV file path (optional)</param>
  public record RunCV(
    int ChannelIndex,
    float StartVoltage_V,
    float Vertex1_V,
    float Vertex2_V,
    float ScanRate_V_s,
    int NCycles,
    string? OutputFile = null);

  /// <summary>
  /// Parameters for running an OCV measurement
  /// </summary>
  /// <param name="ChannelIndex">Zero-based channel index (0-15)</param>
  /// <param name="Duration_s">Measurement duration in seconds</param>
  /// <param name="OutputFile">Output CSV file path (optional)</param>
  public record RunOCV(
    int ChannelIndex,
    float Duration_s,
    string? OutputFile = null);

  /// <summary>
  /// Parameters for running PEIS
  /// </summary>
  /// <param name="ChannelIndex">Zero-based channel index (0-15)</param>
  /// <param name="InitialFrequency_Hz">Starting frequency in Hz</param>
  /// <param name="FinalFrequency_Hz">Ending frequency in Hz</param>
  /// <param name="FrequencyPoints">Number of frequency points</param>
  /// <param name="DcVoltage_V">DC bias voltage in volts</param>
  /// <param name="AcAmplitude_V">AC voltage amplitude in volts</param>
  /// <param name="OutputFile">Output CSV file path (optional)</param>
  public record RunPEIS(
    int ChannelIndex,
    float InitialFrequency_Hz,
    float FinalFrequency_Hz,
    int FrequencyPoints,
    float DcVoltage_V,
    float AcAmplitude_V,
    string? OutputFile = null);

  /// <summary>
  /// Parameters for running GEIS
  /// </summary>
  /// <param name="ChannelIndex">Zero-based channel index (0-15)</param>
  /// <param name="InitialFrequency_Hz">Starting frequency in Hz</param>
  /// <param name="FinalFrequency_Hz">Ending frequency in Hz</param>
  /// <param name="FrequencyPoints">Number of frequency points</param>
  /// <param name="DcCurrent_A">DC bias current in amperes</param>
  /// <param name="AcAmplitude_A">AC current amplitude in amperes</param>
  /// <param name="OutputFile">Output CSV file path (optional)</param>
  public record RunGEIS(
    int ChannelIndex,
    float InitialFrequency_Hz,
    float FinalFrequency_Hz,
    int FrequencyPoints,
    float DcCurrent_A,
    float AcAmplitude_A,
    string? OutputFile = null);

  /// <summary>
  /// Parameters for battery charging using CA (Chrono-Amperometry) technique
  /// CA applies constant voltage steps and measures current response
  /// </summary>
  /// <param name="ChannelIndex">Zero-based channel index (0-15)</param>
  /// <param name="Voltage_V">Applied voltage in volts</param>
  /// <param name="Duration_s">Maximum charge duration in seconds</param>
  /// <param name="RecordInterval_s">Data recording interval in seconds (default: 1.0)</param>
  /// <param name="OutputFile">Output CSV file path (optional)</param>
  public record Charge(
    int ChannelIndex,
    float Voltage_V,
    float Duration_s,
    float RecordInterval_s = 1.0f,
    string? OutputFile = null);

  /// <summary>
  /// Parameters for battery discharging using CA (Chrono-Amperometry) technique
  /// CA applies constant voltage steps and measures current response
  /// </summary>
  /// <param name="ChannelIndex">Zero-based channel index (0-15)</param>
  /// <param name="Voltage_V">Applied voltage in volts</param>
  /// <param name="Duration_s">Maximum discharge duration in seconds</param>
  /// <param name="RecordInterval_s">Data recording interval in seconds (default: 1.0)</param>
  /// <param name="OutputFile">Output CSV file path (optional)</param>
  public record Discharge(
    int ChannelIndex,
    float Voltage_V,
    float Duration_s,
    float RecordInterval_s = 1.0f,
    string? OutputFile = null);
}
