using DeviceControlSoftware;
using Serilog;
using MethodParams = DeviceControlSoftware.MethodParameters;

namespace Biologic.SequenceItems;

/// <summary>
/// Start data polling for a specific channel
/// Enables real-time OPC UA node updates with channel data
/// </summary>
public class StartPollingSequenceItem : SequenceItem
{
  public override void Initialize(Sequence.StateContext context)
  {
    // No initialization needed
  }

  public override Sequence.ResultTypes Execute(Sequence.StateContext context)
  {
    try
    {
      // Get channel from properties (0-based)
      byte channel = byte.Parse(this.Properties?["Channel"]?.ToString() ?? "0");
      
      // Get optional polling interval (default to configured value)
      int pollingInterval = int.Parse(this.Properties?["PollingInterval"]?.ToString() ?? "0");

      // Get ECLabAutomation instance
      if (context.SequenceDispatcher is not ECLabAutomation automation)
      {
        string errorMsg = "SequenceDispatcher is not ECLabAutomation";
        Log.Error(errorMsg);
        context.ResultParameter = new MethodParams.ResultData(false, errorMsg);
        return Sequence.ResultTypes.Error;
      }

      Log.Information("Starting data polling for channel {Channel}", channel);

      // Start polling
      automation.StartPolling(channel, pollingInterval);

      context.ResultParameter = new MethodParams.ResultData(true, $"Started polling for channel {channel}");
      return Sequence.ResultTypes.Next;
    }
    catch (Exception ex)
    {
      Log.Error(ex, "Error starting data polling");
      context.ResultParameter = new MethodParams.ResultData(false, ex.Message);
      return Sequence.ResultTypes.Error;
    }
  }

  public override void Deinitialize(Sequence.StateContext context)
  {
    // No cleanup needed
  }
}
