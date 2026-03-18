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

      if (ecLabDevice.IsConnected && ecLabDevice.HasAvailableChannels)
      {
        Log.Information(
          "ECLab device is already connected and initialized with {ChannelCount} channel(s); skipping duplicate connect",
          ecLabDevice.GetProperty("ChannelCount"));

        if (context.SequenceDispatcher is ECLabAutomation alreadyConnectedAutomation)
        {
          alreadyConnectedAutomation.EnsurePollingForConnectedChannels();
        }

        DeviceControlSoftware.MethodParameters.ResultData alreadyConnected = new(true, "Device already connected");
        context.ResultParameter = alreadyConnected;
        return Sequence.ResultTypes.Next;
      }

      // Open device (this will open communication and initialize)
      ecLabDevice.Open();

      // Check if connection and initialization succeeded
      bool isOpen = ecLabDevice.GetProperty("IsOpen") is bool open && open;
      if (!isOpen)
      {
        DeviceControlSoftware.MethodParameters.ResultData failure = new(false, "Failed to connect to device");
        context.ResultParameter = failure;
        return Sequence.ResultTypes.Error;
      }

      // Verify that we have at least one channel available (firmware loaded successfully)
      int channelCount = ecLabDevice.GetProperty("ChannelCount") is int count ? count : 0;
      if (channelCount == 0)
      {
        Log.Error("No channels available - firmware loading may have failed");
        DeviceControlSoftware.MethodParameters.ResultData failure = new(false, "Device connected but no channels available (firmware loading failed)");
        context.ResultParameter = failure;
        return Sequence.ResultTypes.Error;
      }

      Log.Information("Successfully connected to ECLab device with {ChannelCount} channel(s)", channelCount);

      // Auto-start polling if the system is ECLabAutomation and polling is enabled
      if (context.SequenceDispatcher is ECLabAutomation automation && automation.IsPollingEnabled)
      {
        try
        {
          automation.EnsurePollingForConnectedChannels();
        }
        catch (Exception ex)
        {
          Log.Warning(ex, "Failed to auto-start polling - continuing without real-time data updates");
        }
      }

      DeviceControlSoftware.MethodParameters.ResultData success = new(true, "Device connected successfully");
      context.ResultParameter = success;
      Log.Information("The status of the operation is: {Result}", success.Result);
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
