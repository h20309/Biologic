using DeviceControlSoftware;
using Serilog;
using MethodParams = DeviceControlSoftware.MethodParameters;

namespace Biologic.SequenceItems;

/// <summary>
/// Stop data polling for a specific channel
/// </summary>
public class StopPollingSequenceItem : SequenceItem
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

      // Get ECLabAutomation instance
      if (context.SequenceDispatcher is not ECLabAutomation automation)
      {
        string errorMsg = "SequenceDispatcher is not ECLabAutomation";
        Log.Error(errorMsg);
        context.ResultParameter = new MethodParams.ResultData(false, errorMsg);
        return Sequence.ResultTypes.Error;
      }

      Log.Information("Stopping data polling for channel {Channel}", channel);

      // Stop polling
      automation.StopPolling(channel);

      context.ResultParameter = new MethodParams.ResultData(true, $"Stopped polling for channel {channel}");
      return Sequence.ResultTypes.Next;
    }
    catch (Exception ex)
    {
      Log.Error(ex, "Error stopping data polling");
      context.ResultParameter = new MethodParams.ResultData(false, ex.Message);
      return Sequence.ResultTypes.Error;
    }
  }

  public override void Deinitialize(Sequence.StateContext context)
  {
    // No cleanup needed
  }
}
