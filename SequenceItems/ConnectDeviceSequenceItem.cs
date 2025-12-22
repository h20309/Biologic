using Biologic.Devices;
using Biologic.Native;
using DeviceControlSoftware;
using Serilog;

namespace Biologic.SequenceItems;

/// <summary>
/// Connect to BioLogic device
/// Opens communication and initializes the device
/// </summary>
public class ConnectDeviceSequenceItem : SequenceItem
{
  public override void Initialize(Sequence.StateContext context)
  {
    // No initialization needed
  }

  public override Sequence.ResultTypes Execute(Sequence.StateContext context)
  {
    try
    {
      var device = context.SequenceDispatcher.Devices[ECLabDevice.DeviceName];
      if (device == null || device is not ECLabDevice ecLabDevice)
      {
        throw new InvalidOperationException($"ECLab device not found: {ECLabDevice.DeviceName}");
      }

      Log.Information("Connecting to ECLab device");

      // Open device (this will open communication and initialize)
      ecLabDevice.Open();

      // Check if connection succeeded
      bool isOpen = ecLabDevice.GetProperty("IsOpen") is bool open && open;
      if (!isOpen)
      {
        DeviceControlSoftware.MethodParameters.ResultData failure = new(false, "Failed to connect to device");
        context.ResultParameter = failure;
        return Sequence.ResultTypes.Error;
      }

      Log.Information("Successfully connected to ECLab device");

      DeviceControlSoftware.MethodParameters.ResultData success = new(true, "Device connected successfully");
      context.ResultParameter = success;
      return Sequence.ResultTypes.Next;
    }
    catch (ECLibException ex)
    {
      Log.Error("Connection failed: {ErrorMessage} (Code: {ErrorCode})", ex.Message, ex.ErrorCode);

      DeviceControlSoftware.MethodParameters.ResultData failure = new(false, ex.Message);
      context.ResultParameter = failure;
      return Sequence.ResultTypes.Error;
    }
    catch (Exception ex)
    {
      Log.Error(ex, "Unexpected error connecting to device");

      DeviceControlSoftware.MethodParameters.ResultData failure = new(false, ex.Message);
      context.ResultParameter = failure;
      return Sequence.ResultTypes.Error;
    }
  }

  public override void Deinitialize(Sequence.StateContext context)
  {
    // No cleanup needed
  }
}
