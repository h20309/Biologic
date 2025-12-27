using Biologic.Devices;
using Biologic.Native;
using DeviceControlSoftware;
using Serilog;

namespace Biologic.SequenceItems;

/// <summary>
/// Load firmware to specified channel(s)
/// </summary>
public class LoadFirmwareSequenceItem : SequenceItem
{
  private byte[]? _channels;
  private bool _loadAll = false;
  private bool _force = false;
  private bool _showGauge = false;

  public override void Initialize(Sequence.StateContext context)
  {
    if (this.Properties == null)
      throw new ArgumentNullException(nameof(this.Properties));

    // Check if loading all channels or specific channels
    this._loadAll = Convert.ToBoolean(this.Properties.GetValueOrDefault("LoadAll") ?? false);

    if (!this._loadAll)
    {
      var channelValue = this.Properties.GetValueOrDefault("Channels");
      if (channelValue is string channelStr)
      {
        // Parse comma-separated channel list
        this._channels = channelStr.Split(',')
          .Select(s => Convert.ToByte(s.Trim()))
          .ToArray();
      }
      else if (channelValue is int channelInt)
      {
        this._channels = new[] { (byte)channelInt };
      }
      else
      {
        this._channels = new[] { (byte)0 };
      }
    }

    this._force = Convert.ToBoolean(this.Properties.GetValueOrDefault("Force") ?? false);
    this._showGauge = Convert.ToBoolean(this.Properties.GetValueOrDefault("ShowGauge") ?? false);
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

      bool success;
      string message;

      if (this._loadAll)
      {
        Log.Information("Loading firmware to all channels (Force: {Force}, ShowGauge: {ShowGauge})", this._force, this._showGauge);
        success = ecLabDevice.LoadFirmwareAllChannels(this._force, this._showGauge);
        message = success ? "Firmware loaded to all channels" : "Failed to load firmware to some channels";
      }
      else
      {
        if (this._channels == null || this._channels.Length == 0)
        {
          throw new InvalidOperationException("No channels specified for firmware loading");
        }

        Log.Information("Loading firmware to channel(s): {Channels} (Force: {Force}, ShowGauge: {ShowGauge})",
          string.Join(", ", this._channels), this._force, this._showGauge);

        success = true;
        foreach (byte channel in this._channels)
        {
          bool channelSuccess = ecLabDevice.LoadFirmware(channel, this._force, this._showGauge);
          if (!channelSuccess)
          {
            Log.Error("Failed to load firmware for channel {Channel}", channel);
            success = false;
          }
        }

        message = success
          ? $"Firmware loaded to channel(s): {string.Join(", ", this._channels)}"
          : "Failed to load firmware to some channels";
      }

      if (!success)
      {
        DeviceControlSoftware.MethodParameters.ResultData failure = new(false, message);
        context.ResultParameter = failure;
        return Sequence.ResultTypes.Error;
      }

      Log.Information("Firmware loading completed successfully");

      DeviceControlSoftware.MethodParameters.ResultData successResult = new(true, message);
      context.ResultParameter = successResult;
      return Sequence.ResultTypes.Next;
    }
    catch (ECLibException ex)
    {
      Log.Error("LoadFirmware failed: {ErrorMessage} (Code: {ErrorCode})", ex.Message, ex.ErrorCode);

      DeviceControlSoftware.MethodParameters.ResultData failure = new(false, ex.Message);
      context.ResultParameter = failure;
      return Sequence.ResultTypes.Error;
    }
    catch (Exception ex)
    {
      Log.Error(ex, "Unexpected error loading firmware");

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
