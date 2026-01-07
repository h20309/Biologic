using Biologic.Communications;
using Biologic.Devices;
using Biologic.SequenceItems;
using DeviceControlSoftware;
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
    // Get device configuration from settings
    string deviceName = this.Settings?.Properties["EC-LAB"]?["DeviceName"]?.ToString() ?? "EC-LAB";
    string address = this.Settings?.Properties["EC-LAB"]?["Address"]?.ToString() ?? "USB0";
    int timeoutMs = int.Parse(this.Settings?.Properties["EC-LAB"]?["TimeoutMs"]?.ToString() ?? "5000");
    string? techniquesPath = this.Settings?.Properties["EC-LAB"]?["TechniquesPath"]?.ToString();

    // Create communication
    this.communication = new ECLabCommunication(address, timeoutMs);

    // Create device with techniques path from configuration
    // TechniquesPath contains both technique files and firmware files
    this.device = new ECLabDevice(this.communication, techniquesPath);

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
    this.SequenceController.AddMethodParameterCreator("RunCV", (x) => JsonSerializer.Deserialize<MethodParameters.RunCV>(x ?? string.Empty)!);
    this.SequenceController.AddMethodParameterCreator("RunOCV", (x) => JsonSerializer.Deserialize<MethodParameters.RunOCV>(x ?? string.Empty)!);
    this.SequenceController.AddMethodParameterCreator("RunPEIS", (x) => JsonSerializer.Deserialize<MethodParameters.RunPEIS>(x ?? string.Empty)!);
    this.SequenceController.AddMethodParameterCreator("RunGEIS", (x) => JsonSerializer.Deserialize<MethodParameters.RunGEIS>(x ?? string.Empty)!);
    this.SequenceController.AddMethodParameterCreator("Charge", (x) => JsonSerializer.Deserialize<MethodParameters.Charge>(x ?? string.Empty)!);
    this.SequenceController.AddMethodParameterCreator("Discharge", (x) => JsonSerializer.Deserialize<MethodParameters.Discharge>(x ?? string.Empty)!);
  }

  /// <summary>
  /// Get ECLabDevice instance for derived classes
  /// </summary>
  protected ECLabDevice? GetECLabDevice()
  {
    return this.device;
  }
}
