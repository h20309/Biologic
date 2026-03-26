using Biologic.Communications;
using Biologic.Devices;
using Biologic.Native;
using Biologic.SequenceItems;
using DeviceControlSoftware;
using Serilog;
using System.Text.Json;

namespace Biologic;

/// <summary>
/// BioLogic electrochemistry system dispatcher
/// </summary>
public class ECLabSystem : SequenceDispatcher
{
  protected override string Name => "BiologicSystem";

  private ECLabCommunication? communication;
  private ECLabDevice? device;

  public override void EntryViewModel(IViewModelMenuHost menu)
  {
    // TODO: Add view model if needed
  }

  protected override void Setup()
  {
    var eclabSettings = this.Settings?.Properties.TryGetValue("EC-LAB", out var ecLabProperties) == true
      ? ecLabProperties
      : null;

    // Get device configuration from settings
    string deviceName = eclabSettings?.GetValueOrDefault("DeviceName")?.ToString() ?? "EC-LAB";
    string adapterMode = eclabSettings?.GetValueOrDefault("AdapterMode")?.ToString() ?? "Real";
    string address = eclabSettings?.GetValueOrDefault("Address")?.ToString() ?? "USB0";
    int timeoutMs = int.Parse(eclabSettings?.GetValueOrDefault("TimeoutMs")?.ToString() ?? "5000");
    string? techniquesPath = eclabSettings?.GetValueOrDefault("TechniquesPath")?.ToString();
    string? eclibDirectory = eclabSettings?.GetValueOrDefault("ECLibDirectory")?.ToString()
      ?? eclabSettings?.GetValueOrDefault("LibraryPath")?.ToString();

    // Configure native backend based on AdapterMode (mirrors Nuvoton pattern)
    if (string.Equals(adapterMode, "Mock", StringComparison.OrdinalIgnoreCase))
    {
      int mockRunDuration = int.TryParse(eclabSettings?.GetValueOrDefault("MockRunDurationSeconds")?.ToString(), out int dur) ? dur : 10;
      ECLibApi.SetNative(new MockECLibNative(channelCount: 1, techniqueRunDurationSeconds: mockRunDuration));
      Log.Information("ECLabSystem: Using MockECLibNative adapter (MockRunDurationSeconds={Duration})", mockRunDuration);
    }
    else
    {
      ECLibApi.SetNative(new RealECLibNative());
    }

    // Create communication
    this.communication = new ECLabCommunication(address, timeoutMs);

    // Create device with techniques path from configuration
    // TechniquesPath contains both technique files and firmware files
    this.device = new ECLabDevice(
      this.communication,
      techniquesPath,
      eclibDirectory,
      Directory.GetCurrentDirectory());

    this.ConfigureNativeLibraryDirectory(this.device.ECLibDirectory);

    // Register device with configured name (from setting.json)
    // This name must match the node name in OPC UA NodeSet XML file
    this.devices.Add(deviceName, this.device);

    // Register sequence creators
    // Device management
    this.AddSequenceCreator("ConnectDevice", (prop) => new ConnectDeviceSequenceItem { Properties = prop });
    this.AddSequenceCreator("DisconnectDevice", (prop) => new DisconnectDeviceSequenceItem { Properties = prop });
    this.AddSequenceCreator("LoadFirmware", (prop) => new LoadFirmwareSequenceItem { Properties = prop });

    // Channel control
    this.AddSequenceCreator("StartChannel", (prop) => new StartChannelSequenceItem { Properties = prop });
    this.AddSequenceCreator("StopChannel", (prop) => new StopChannelSequenceItem { Properties = prop });
    this.AddSequenceCreator("ForceStopBiologicChannel", (prop) => new ForceStopBiologicChannelSequenceItem { Properties = prop });

    // Technique loading
    this.AddSequenceCreator("LoadTechnique", (prop) => new LoadTechniqueSequenceItem { Properties = prop });

    // Battery testing
    this.AddSequenceCreator("Charge", (prop) => new ChargeSequenceItem { Properties = prop });
    this.AddSequenceCreator("Discharge", (prop) => new DischargeSequenceItem { Properties = prop });

    // Electrochemical Impedance Spectroscopy
    this.AddSequenceCreator("GEIS", (prop) => new GEISSequenceItem { Properties = prop });
    this.AddSequenceCreator("PEIS", (prop) => new PEISSequenceItem { Properties = prop });

    // Data acquisition
    this.AddSequenceCreator("GetData", (prop) => new GetDataSequenceItem { Properties = prop });
    this.AddSequenceCreator("WaitForCompletion", (prop) => new WaitForCompletionSequenceItem { Properties = prop });
    
    // Data polling control
    this.AddSequenceCreator("StartPolling", (prop) => new StartPollingSequenceItem { Properties = prop });
    this.AddSequenceCreator("StopPolling", (prop) => new StopPollingSequenceItem { Properties = prop });

    // Register method parameter creators for OPC UA Methods
    // These enable dynamic technique execution via OPC UA with strongly-typed parameters
    this.SequenceController.AddMethodParameterCreator("RunCV", (x) => DeserializeMethodParameter<MethodParameters.RunCV>(x)!);
    this.SequenceController.AddMethodParameterCreator("RunOCV", (x) => DeserializeMethodParameter<MethodParameters.RunOCV>(x)!);
    this.SequenceController.AddMethodParameterCreator("RunPEIS", (x) => DeserializeMethodParameter<MethodParameters.RunPEIS>(x)!);
    this.SequenceController.AddMethodParameterCreator("RunGEIS", (x) => DeserializeMethodParameter<MethodParameters.RunGEIS>(x)!);
    this.SequenceController.AddMethodParameterCreator("Charge", (x) => DeserializeMethodParameter<MethodParameters.Charge>(x)!);
    this.SequenceController.AddMethodParameterCreator("Discharge", (x) => DeserializeMethodParameter<MethodParameters.Discharge>(x)!);
    this.SequenceController.AddMethodParameterCreator(
      "ForceStopChannel",
      (x) => DeserializeMethodParameter<MethodParameters.ForceStopChannel>(x)!);
  }

  /// <summary>
  /// Get ECLabDevice instance for derived classes
  /// </summary>
  protected ECLabDevice? GetECLabDevice()
  {
    return this.device;
  }

  private void ConfigureNativeLibraryDirectory(string libraryDirectory)
  {
    if (string.IsNullOrWhiteSpace(libraryDirectory) || !Directory.Exists(libraryDirectory))
    {
      return;
    }

    ECLibNative.ConfigurePreferredLibraryDirectory(libraryDirectory);

    string currentPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
    string[] pathEntries = currentPath.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
    if (pathEntries.Any(entry => string.Equals(entry, libraryDirectory, StringComparison.OrdinalIgnoreCase)))
    {
      return;
    }

    string updatedPath = string.IsNullOrWhiteSpace(currentPath)
      ? libraryDirectory
      : string.Join(Path.PathSeparator, new[] { libraryDirectory, currentPath });

    Environment.SetEnvironmentVariable("PATH", updatedPath);
  }

  private static object? DeserializeMethodParameter<T>(string? json)
  {
    if (string.IsNullOrWhiteSpace(json))
    {
      return null;
    }

    return JsonSerializer.Deserialize<T>(json);
  }
}
