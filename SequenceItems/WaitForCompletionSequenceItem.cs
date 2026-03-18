using Biologic.Native;
using DeviceControlSoftware;
using Serilog;

namespace Biologic.SequenceItems;

/// <summary>
/// Wait for technique execution to complete on a channel
/// Uses ECLibApi to poll channel status
/// </summary>
public class WaitForCompletionSequenceItem : SequenceItem
{
  private int _deviceId;
  private byte _channelIndex;
  private int _timeoutSeconds = 300; // Default 5 minutes
  private int _pollIntervalMs = 1000; // Default 1 second

  public override void Initialize(Sequence.StateContext context)
  {
    if (Properties == null)
      throw new ArgumentNullException(nameof(Properties));

    _channelIndex = Convert.ToByte(Properties.GetValueOrDefault("ChannelIndex") ?? 0);
    if (TryGetMethodChannelIndex(context.MethodParameter, out byte methodChannelIndex))
    {
      _channelIndex = methodChannelIndex;
    }
    _timeoutSeconds = Convert.ToInt32(Properties.GetValueOrDefault("TimeoutSeconds") ?? 300);
    _pollIntervalMs = Convert.ToInt32(Properties.GetValueOrDefault("PollIntervalMs") ?? 1000);

    // Get device ID from Device properties
    var device = context.SequenceDispatcher.Devices.Values.FirstOrDefault();
    if (device != null && device.GetProperty("DeviceId") is int deviceId)
    {
      _deviceId = deviceId;
    }
    else
    {
      throw new InvalidOperationException("Device or DeviceId not found in context");
    }
  }

  public override Sequence.ResultTypes Execute(Sequence.StateContext context)
  {
    try
    {
      Log.Information("Waiting for technique completion on channel {Channel} (timeout: {Timeout}s)", 
          _channelIndex, _timeoutSeconds);

      var startTime = DateTime.Now;
      double elapsedSeconds = 0;
      CurrentValues currentValues = default;

      while (elapsedSeconds < _timeoutSeconds)
      {
        // Check if cancellation requested
        if (context.Token.IsCancellationRequested)
        {
          Log.Warning("Wait for completion cancelled on channel {Channel}", _channelIndex);

          context.ResultParameter = new
          {
            Success = false,
            Message = "Operation cancelled"
          };

          return Sequence.ResultTypes.Error;
        }

        // Get current channel status
        try
        {
          currentValues = ECLibApi.GetCurrentValues(_deviceId, _channelIndex + 1); // Convert to 1-based
        }
        catch (ECLibException ex)
        {
          Log.Error("GetCurrentValues failed: {ErrorMessage}", ex.Message);

          context.ResultParameter = new
          {
            Success = false,
            Message = $"Failed to get channel status: {ex.Message}"
          };

          return Sequence.ResultTypes.Error;
        }

        // Check if channel has stopped
        if (currentValues.State == (int)PROG_STATE.STOP)
        {
          Log.Information(
              "Technique completed on channel {Channel} after {Elapsed} seconds",
              _channelIndex,
              elapsedSeconds);

          context.ResultParameter = new
          {
            Success = true,
            Message = "Technique completed",
            ElapsedTime = elapsedSeconds,
            FinalState = ((PROG_STATE)currentValues.State).ToString()
          };

          return Sequence.ResultTypes.Next;
        }

        // Log current state periodically
        if ((int)elapsedSeconds % 10 == 0) // Every 10 seconds
        {
          Log.Debug(
              "Channel {Channel} - State: {State}, Time: {Time}s, Ewe: {Ewe}V",
              _channelIndex,
              (PROG_STATE)currentValues.State,
              currentValues.ElapsedTime,
              currentValues.Ewe);
        }

        // Wait before next poll
        Thread.Sleep(_pollIntervalMs);

        elapsedSeconds = (DateTime.Now - startTime).TotalSeconds;
      }

      // Timeout reached
      Log.Warning(
          "Timeout waiting for technique completion on channel {Channel}. Current state: {State}",
          _channelIndex,
          (PROG_STATE)currentValues.State);

      context.ResultParameter = new
      {
        Success = false,
        Message = $"Timeout after {_timeoutSeconds} seconds",
        CurrentState = ((PROG_STATE)currentValues.State).ToString(),
        ElapsedTime = elapsedSeconds
      };

      return Sequence.ResultTypes.Error;
    }
    catch (Exception ex)
    {
      Log.Error(ex, "Unexpected error in WaitForCompletion");

      context.ResultParameter = new
      {
        Success = false,
        Message = $"Unexpected error: {ex.Message}"
      };

      return Sequence.ResultTypes.Error;
    }
  }

  public override void Deinitialize(Sequence.StateContext context)
  {
    // No cleanup needed
  }

  private static bool TryGetMethodChannelIndex(object? methodParameter, out byte channelIndex)
  {
    channelIndex = 0;
    switch (methodParameter)
    {
      case Biologic.MethodParameters.RunCV cv:
        channelIndex = Convert.ToByte(cv.ChannelIndex);
        return true;
      case Biologic.MethodParameters.RunOCV ocv:
        channelIndex = Convert.ToByte(ocv.ChannelIndex);
        return true;
      case Biologic.MethodParameters.RunPEIS peis:
        channelIndex = Convert.ToByte(peis.ChannelIndex);
        return true;
      case Biologic.MethodParameters.RunGEIS geis:
        channelIndex = Convert.ToByte(geis.ChannelIndex);
        return true;
      default:
        return false;
    }
  }
}
