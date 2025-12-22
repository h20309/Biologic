using Biologic.Devices;
using Biologic.Native;
using DeviceControlSoftware;
using Serilog;

namespace Biologic.SequenceItems;

/// <summary>
/// Disconnect from BioLogic device
/// Closes communication and releases device resources
/// </summary>
public class DisconnectDeviceSequenceItem : SequenceItem
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

      Log.Information("Disconnecting from ECLab device");

      // Close device (this will close communication)
      ecLabDevice.Close();

      Log.Information("Successfully disconnected from ECLab device");

      DeviceControlSoftware.MethodParameters.ResultData success = new(true, "Device disconnected successfully");
      context.ResultParameter = success;
      return Sequence.ResultTypes.Next;
    }
    catch (ECLibException ex)
    {
      Log.Error("Disconnection failed: {ErrorMessage} (Code: {ErrorCode})", ex.Message, ex.ErrorCode);

      DeviceControlSoftware.MethodParameters.ResultData failure = new(false, ex.Message);
      context.ResultParameter = failure;
      return Sequence.ResultTypes.Error;
    }
    catch (Exception ex)
    {
      Log.Error(ex, "Unexpected error disconnecting from device");

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
