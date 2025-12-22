using Biologic.Native;
using DeviceControlSoftware;
using Serilog;

namespace Biologic.Communications;

/// <summary>
/// ECLab device communication handler
/// Manages connection lifecycle to BioLogic devices via EClib64.dll
/// </summary>
public class ECLabCommunication : Communication
{
  private readonly string address;
  private readonly byte timeoutSeconds;
  private int deviceId = -1;
  private DeviceInfo deviceInfo;
  private bool isOpen;
  private bool isError;

  public int DeviceId => this.deviceId;
  public DeviceInfo DeviceInfo => this.deviceInfo;
  public bool IsOpen => this.isOpen;

  public override bool IsError => this.isError;

  public ECLabCommunication(string address = "USB0", int timeoutMs = 5000)
  {
    this.address = address;
    int timeoutSeconds = Math.Max(1, timeoutMs / 1000);
    this.timeoutSeconds = (byte)Math.Min(255, timeoutSeconds);

    Log.Information(
        "ECLabCommunication created for address: {Address}, Timeout: {TimeoutSeconds}s",
        address, this.timeoutSeconds);
  }

  public override bool Open()
  {
    try
    {
      if (this.isOpen)
      {
        Log.Warning("ECLab communication already open");
        return true;
      }

      Log.Information("Connecting to ECLab device at {Address}", this.address);

      try
      {
        (this.deviceId, this.deviceInfo) = ECLibApi.Connect(this.address, this.timeoutSeconds);
      }
      catch (ECLibException ex)
      {
        Log.Error(
            "Failed to connect to ECLab device at {Address}. Error code: {ErrorCode}, Message: {ErrorMessage}",
            this.address, ex.ErrorCode, ex.Message);
        this.isError = true;
        return false;
      }

      this.isOpen = true;
      this.isError = false;

      if (this.Node != null)
      {
        this.Status = "Connected";
      }

      Log.Information(
          "Connected to ECLab device ID: {DeviceId}, Type: {DeviceCode}, Channels: {Channels}",
          this.deviceId,
          (DEVICE)this.deviceInfo.DeviceCode,
          this.deviceInfo.NumberOfChannels);
      
      Log.Information(
          "Note: Firmware and FPGA loading happens in ECLabDevice.LoadFirmware(), not during connection");

      return true;
    }
    catch (DllNotFoundException dllEx)
    {
      Log.Error(dllEx, "ECLib64.dll not found. Please ensure EC-Lab Development Package is installed or ECLib64.dll is in PATH");
      Log.Error("Current directory: {WorkingDir}", Directory.GetCurrentDirectory());
      this.isError = true;
      this.isOpen = false;
      return false;
    }
    catch (Exception ex)
    {
      Log.Error(ex, "Exception occurred while opening ECLab communication");
      this.isError = true;
      this.isOpen = false;
      return false;
    }
  }

  public override void Close()
  {
    try
    {
      if (this.deviceId >= 0)
      {
        Log.Information("Disconnecting from ECLab device {DeviceId}", this.deviceId);
        ECLibApi.Disconnect(this.deviceId);
        this.deviceId = -1;
      }

      this.isOpen = false;
      this.isError = false;

      if (this.Node != null)
      {
        this.Status = "Disconnected";
      }
    }
    catch (Exception ex)
    {
      Log.Error(ex, "Error disconnecting from ECLab device");
      this.isError = true;
    }
  }

  /// <summary>
  /// Test device connection health
  /// </summary>
  public bool TestConnection()
  {
    if (!this.isOpen || this.deviceId < 0)
      return false;

    try
    {
      bool isConnected = ECLibApi.TestConnection(this.deviceId);

      if (!isConnected)
      {
        this.isError = true;
        return false;
      }

      return true;
    }
    catch (Exception ex)
    {
      Log.Error(ex, "Exception during connection test");
      this.isError = true;
      return false;
    }
  }
}
