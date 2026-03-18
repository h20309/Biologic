using Biologic.Native;
using DeviceControlSoftware;
using Serilog;

namespace Biologic.SequenceItems;

/// <summary>
/// Stop channel execution
/// Uses ECLibApi for type-safe channel control
/// </summary>
public class StopChannelSequenceItem : SequenceItem
{
  private int _deviceId;
  private byte _channelIndex;

  public override void Initialize(Sequence.StateContext context)
  {
    if (Properties == null)
      throw new ArgumentNullException(nameof(Properties));

    _channelIndex = Convert.ToByte(Properties.GetValueOrDefault("ChannelIndex") ?? 0);
    if (TryGetMethodChannelIndex(context.MethodParameter, out byte methodChannelIndex))
    {
      _channelIndex = methodChannelIndex;
    }

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
      Log.Information("Stopping channel {Channel}", _channelIndex);

      // Convert to 1-based index for ECLibApi
      ECLibApi.StopChannel(_deviceId, _channelIndex + 1);

      Log.Information("Channel {Channel} stopped successfully", _channelIndex);

      context.ResultParameter = new
      {
        Success = true,
        Message = $"Channel {_channelIndex} stopped"
      };

      return Sequence.ResultTypes.Next;
    }
    catch (ECLibException ex)
    {
      Log.Error("StopChannel failed: {ErrorMessage} (Code: {ErrorCode})", ex.Message, ex.ErrorCode);

      context.ResultParameter = new
      {
        Success = false,
        Message = $"Failed to stop channel: {ex.Message}"
      };

      return Sequence.ResultTypes.Error;
    }
    catch (Exception ex)
    {
      Log.Error(ex, "Unexpected error in StopChannel");

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
      case Biologic.MethodParameters.Charge charge:
        channelIndex = Convert.ToByte(charge.ChannelIndex);
        return true;
      case Biologic.MethodParameters.Discharge discharge:
        channelIndex = Convert.ToByte(discharge.ChannelIndex);
        return true;
      case Biologic.MethodParameters.ForceStopChannel forceStop:
        channelIndex = Convert.ToByte(forceStop.ChannelIndex);
        return true;
      default:
        return false;
    }
  }
}
