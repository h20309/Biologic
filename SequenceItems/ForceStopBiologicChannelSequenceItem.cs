using Biologic.Native;
using DeviceControlSoftware;
using Serilog;

namespace Biologic.SequenceItems;

/// <summary>
/// Force stop the currently running Biologic sequence on a specific channel.
/// This stops the hardware channel and aborts matching method sequence contexts.
/// </summary>
public class ForceStopBiologicChannelSequenceItem : SequenceItem
{
  private static readonly HashSet<string> TargetMethodNames = new(StringComparer.OrdinalIgnoreCase)
  {
    "RunCV",
    "RunOCV",
    "RunPEIS",
    "RunGEIS",
    "Charge",
    "Discharge"
  };

  private int _deviceId;
  private byte _channelIndex;

  public override void Initialize(Sequence.StateContext context)
  {
    if (Properties == null)
      throw new ArgumentNullException(nameof(Properties));

    _channelIndex = Convert.ToByte(Properties.GetValueOrDefault("ChannelIndex") ?? 0);

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
      Log.Information("Force stopping Biologic channel {Channel}", _channelIndex);

      try
      {
        ECLibApi.StopChannel(_deviceId, _channelIndex + 1);
      }
      catch (ECLibException ex)
      {
        Log.Warning("StopChannel during force stop for channel {Channel} reported: {Error}", _channelIndex, ex.Message);
      }

      int abortedContexts = 0;
      foreach (var sequence in context.SequenceDispatcher.SequenceController.Methods.Where(sequence => TargetMethodNames.Contains(sequence.TriggerData)))
      {
        foreach (var state in sequence.Contexts.Where(state => state.IsBusy && TargetsChannel(state, _channelIndex)).ToArray())
        {
          state.Abort();
          abortedContexts++;
        }
      }

      context.ResultParameter = new
      {
        Success = true,
        Message = $"Force-stopped Biologic channel {_channelIndex}",
        ChannelIndex = _channelIndex,
        AbortedContexts = abortedContexts
      };

      return Sequence.ResultTypes.Next;
    }
    catch (Exception ex)
    {
      Log.Error(ex, "Unexpected error in ForceStopBiologicChannel");

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

  private static bool TargetsChannel(Sequence.StateContext context, byte channel)
  {
    if (TryGetChannelIndex(context.MethodParameter, out byte methodChannel))
    {
      return methodChannel == channel;
    }

    foreach (var item in context.Sequence.Items)
    {
      if (item.Properties != null && item.Properties.TryGetValue("ChannelIndex", out object? value) && Convert.ToByte(value) == channel)
      {
        return true;
      }
    }

    return false;
  }

  private static bool TryGetChannelIndex(object? methodParameter, out byte channel)
  {
    channel = 0;
    if (methodParameter == null)
    {
      return false;
    }

    var property = methodParameter.GetType().GetProperty("ChannelIndex");
    if (property == null)
    {
      return false;
    }

    object? value = property.GetValue(methodParameter);
    if (value == null)
    {
      return false;
    }

    channel = Convert.ToByte(value);
    return true;
  }
}