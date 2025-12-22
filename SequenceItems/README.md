# Biologic SequenceItems Documentation

This document describes all available SequenceItems for the Biologic electrochemistry system.

## Device Management

### ConnectDeviceSequenceItem
Connects to the BioLogic device and initializes communication.

**Type**: `ConnectDevice`

**Properties**: None required

**Example**:
```json
{
  "Type": "ConnectDevice"
}
```

**Result**:
- `Success`: true/false
- `Message`: Status message

---

### DisconnectDeviceSequenceItem
Disconnects from the BioLogic device and closes communication.

**Type**: `DisconnectDevice`

**Properties**: None required

**Example**:
```json
{
  "Type": "DisconnectDevice"
}
```

**Result**:
- `Success`: true/false
- `Message`: Status message

---

### LoadFirmwareSequenceItem
Loads firmware to specified channel(s) or all channels.

**Type**: `LoadFirmware`

**Properties**:
- `LoadAll` (bool, optional): Load firmware to all channels (default: false)
- `Channels` (string/int, optional): Comma-separated list of channels (e.g., "0,1,2") or single channel
- `Force` (bool, optional): Force firmware reload (default: false)
- `ShowGauge` (bool, optional): Show progress gauge (default: false)

**Example**:
```json
{
  "Type": "LoadFirmware",
  "Properties": {
    "LoadAll": true,
    "Force": false,
    "ShowGauge": true
  }
}
```

**Result**:
- `Success`: true/false
- `Message`: Status message with channel information

---

## Channel Control

### StartChannelSequenceItem
Starts execution on a channel (technique must be loaded first).

**Type**: `StartChannel`

**Properties**:
- `ChannelIndex` (int, required): Zero-based channel index (0-15)

**Example**:
```json
{
  "Type": "StartChannel",
  "Properties": {
    "ChannelIndex": 0
  }
}
```

**Result**:
- `Success`: true/false
- `Message`: Status message

---

### StopChannelSequenceItem
Stops execution on a channel.

**Type**: `StopChannel`

**Properties**:
- `ChannelIndex` (int, required): Zero-based channel index (0-15)

**Example**:
```json
{
  "Type": "StopChannel",
  "Properties": {
    "ChannelIndex": 0
  }
}
```

**Result**:
- `Success`: true/false
- `Message`: Status message

---

## Technique Loading

### LoadTechniqueSequenceItem
Loads a technique file (.ecc) with parameters to a channel.

**Type**: `LoadTechnique`

**Properties**:
- `ChannelIndex` (int, required): Zero-based channel index (0-15)
- `TechniqueFile` (string, required): Technique filename (e.g., "ocv.ecc", "cv.ecc")
- `Parameters` (object, optional): Dictionary of technique parameters

**Example**:
```json
{
  "Type": "LoadTechnique",
  "Properties": {
    "ChannelIndex": 0,
    "TechniqueFile": "cv.ecc",
    "Parameters": {
      "vs_initial": 0.0,
      "Vertex_1_E": 1.0,
      "Vertex_2_E": -1.0,
      "Scan_Rate": 0.1,
      "N_Cycles": 3
    }
  }
}
```

**Result**:
- `Success`: true/false
- `Message`: Status message

---

## Battery Testing

### ChargeSequenceItem
Charges a battery using galvanostatic (constant current) method via CA technique.

**Type**: `Charge`

**Properties**:
- `ChannelIndex` (int, required): Zero-based channel index (0-15)
- `Current_A` (float, required): Charging current in amperes (positive value)
- `Duration_s` (float, required): Maximum charge duration in seconds
- `CutoffVoltage_V` (float, required): Upper voltage limit in volts

**Example**:
```json
{
  "Type": "Charge",
  "Properties": {
    "ChannelIndex": 0,
    "Current_A": 0.5,
    "Duration_s": 3600,
    "CutoffVoltage_V": 4.2
  }
}
```

**Result**:
- `Success`: true/false
- `Message`: Status message
- `Current_A`: Applied current
- `Duration_s`: Duration
- `CutoffVoltage_V`: Cutoff voltage

**Notes**:
- The technique automatically loads and starts the channel
- Use `WaitForCompletion` to wait for charge completion
- Use `GetData` to retrieve charge data

---

### DischargeSequenceItem
Discharges a battery using galvanostatic (constant current) method via CA technique.

**Type**: `Discharge`

**Properties**:
- `ChannelIndex` (int, required): Zero-based channel index (0-15)
- `Current_A` (float, required): Discharge current in amperes (negative value or will be made negative)
- `Duration_s` (float, required): Maximum discharge duration in seconds
- `CutoffVoltage_V` (float, required): Lower voltage limit in volts

**Example**:
```json
{
  "Type": "Discharge",
  "Properties": {
    "ChannelIndex": 0,
    "Current_A": -0.5,
    "Duration_s": 3600,
    "CutoffVoltage_V": 2.5
  }
}
```

**Result**:
- `Success`: true/false
- `Message`: Status message
- `Current_A`: Applied current
- `Duration_s`: Duration
- `CutoffVoltage_V`: Cutoff voltage

**Notes**:
- The technique automatically loads and starts the channel
- Use `WaitForCompletion` to wait for discharge completion
- Use `GetData` to retrieve discharge data

---

## Electrochemical Impedance Spectroscopy (EIS)

### GEISSequenceItem
Performs Galvanostatic Electrochemical Impedance Spectroscopy (GEIS).
Applies AC current and measures impedance at different frequencies.

**Type**: `GEIS`

**Properties**:
- `ChannelIndex` (int, required): Zero-based channel index (0-15)
- `InitialFrequency_Hz` (float, optional): Starting frequency in Hz (default: 100000)
- `FinalFrequency_Hz` (float, optional): Ending frequency in Hz (default: 0.01)
- `FrequencyPoints` (int, optional): Number of frequency points (default: 10)
- `DcCurrent_A` (float, optional): DC bias current in amperes (default: 0.0)
- `AcAmplitude_A` (float, optional): AC current amplitude in amperes (default: 0.0001)

**Example**:
```json
{
  "Type": "GEIS",
  "Properties": {
    "ChannelIndex": 0,
    "InitialFrequency_Hz": 100000,
    "FinalFrequency_Hz": 0.1,
    "FrequencyPoints": 30,
    "DcCurrent_A": 0.0,
    "AcAmplitude_A": 0.0001
  }
}
```

**Result**:
- `Success`: true/false
- `Message`: Status message
- `InitialFrequency_Hz`: Starting frequency
- `FinalFrequency_Hz`: Ending frequency
- `FrequencyPoints`: Number of points
- `DcCurrent_A`: DC bias current
- `AcAmplitude_A`: AC amplitude

**Notes**:
- Frequency sweep is logarithmic by default
- Use `WaitForCompletion` to wait for measurement completion
- Use `GetData` to retrieve impedance data (frequency, Z_real, Z_imag)

---

### PEISSequenceItem
Performs Potentiostatic Electrochemical Impedance Spectroscopy (PEIS).
Applies AC voltage and measures impedance at different frequencies.

**Type**: `PEIS`

**Properties**:
- `ChannelIndex` (int, required): Zero-based channel index (0-15)
- `InitialFrequency_Hz` (float, optional): Starting frequency in Hz (default: 100000)
- `FinalFrequency_Hz` (float, optional): Ending frequency in Hz (default: 0.01)
- `FrequencyPoints` (int, optional): Number of frequency points (default: 10)
- `DcVoltage_V` (float, optional): DC bias voltage in volts (default: 0.0)
- `AcAmplitude_V` (float, optional): AC voltage amplitude in volts (default: 0.01)

**Example**:
```json
{
  "Type": "PEIS",
  "Properties": {
    "ChannelIndex": 0,
    "InitialFrequency_Hz": 100000,
    "FinalFrequency_Hz": 0.1,
    "FrequencyPoints": 30,
    "DcVoltage_V": 0.0,
    "AcAmplitude_V": 0.01
  }
}
```

**Result**:
- `Success`: true/false
- `Message`: Status message
- `InitialFrequency_Hz`: Starting frequency
- `FinalFrequency_Hz`: Ending frequency
- `FrequencyPoints`: Number of points
- `DcVoltage_V`: DC bias voltage
- `AcAmplitude_V`: AC amplitude

**Notes**:
- Frequency sweep is logarithmic by default
- Use `WaitForCompletion` to wait for measurement completion
- Use `GetData` to retrieve impedance data (frequency, Z_real, Z_imag)

---

## Data Acquisition

### GetDataSequenceItem
Retrieves data from a channel and saves to a file.

**Type**: `GetData`

**Properties**:
- `ChannelIndex` (int, required): Zero-based channel index (0-15)
- `OutputFile` (string, optional): Output filename (default: "data.csv")
- `MaxDataPoints` (int, optional): Maximum number of data points to retrieve (default: 10000)
- `AppendData` (bool, optional): Append to existing file (default: false)

**Example**:
```json
{
  "Type": "GetData",
  "Properties": {
    "ChannelIndex": 0,
    "OutputFile": "experiment_001.csv",
    "MaxDataPoints": 50000,
    "AppendData": false
  }
}
```

**Result**:
- `Success`: true/false
- `Message`: Status message
- `DataPoints`: Number of data points retrieved
- `OutputFile`: Full path to output file

**Output Format**:
CSV file with columns depending on technique:
- Time(s), Ewe(V), I(A), Cycle

---

### WaitForCompletionSequenceItem
Waits for a channel to complete its current technique.

**Type**: `WaitForCompletion`

**Properties**:
- `ChannelIndex` (int, required): Zero-based channel index (0-15)
- `TimeoutSeconds` (int, optional): Maximum wait time in seconds (default: 3600)
- `PollIntervalMs` (int, optional): Polling interval in milliseconds (default: 1000)

**Example**:
```json
{
  "Type": "WaitForCompletion",
  "Properties": {
    "ChannelIndex": 0,
    "TimeoutSeconds": 7200,
    "PollIntervalMs": 500
  }
}
```

**Result**:
- `Success`: true/false
- `Message`: Status message
- `ElapsedSeconds`: Time elapsed waiting

---

## Complete Example Sequences

### Battery Charge-Discharge Cycle
```json
{
  "Name": "Battery Charge-Discharge Test",
  "Items": [
    {
      "Id": "01",
      "Type": "ConnectDevice"
    },
    {
      "Id": "02",
      "Type": "LoadFirmware",
      "Properties": {
        "Channels": "0",
        "Force": false
      }
    },
    {
      "Id": "03",
      "Type": "Charge",
      "Properties": {
        "ChannelIndex": 0,
        "Current_A": 1.0,
        "Duration_s": 3600,
        "CutoffVoltage_V": 4.2
      }
    },
    {
      "Id": "04",
      "Type": "WaitForCompletion",
      "Properties": {
        "ChannelIndex": 0,
        "TimeoutSeconds": 4000
      }
    },
    {
      "Id": "05",
      "Type": "GetData",
      "Properties": {
        "ChannelIndex": 0,
        "OutputFile": "charge_data.csv"
      }
    },
    {
      "Id": "06",
      "Type": "Delay",
      "Properties": {
        "DelayTime": 300
      }
    },
    {
      "Id": "07",
      "Type": "Discharge",
      "Properties": {
        "ChannelIndex": 0,
        "Current_A": -1.0,
        "Duration_s": 3600,
        "CutoffVoltage_V": 2.5
      }
    },
    {
      "Id": "08",
      "Type": "WaitForCompletion",
      "Properties": {
        "ChannelIndex": 0,
        "TimeoutSeconds": 4000
      }
    },
    {
      "Id": "09",
      "Type": "GetData",
      "Properties": {
        "ChannelIndex": 0,
        "OutputFile": "discharge_data.csv"
      }
    },
    {
      "Id": "10",
      "Type": "DisconnectDevice"
    }
  ]
}
```

### EIS Measurement
```json
{
  "Name": "PEIS Measurement",
  "Items": [
    {
      "Id": "01",
      "Type": "ConnectDevice"
    },
    {
      "Id": "02",
      "Type": "LoadFirmware",
      "Properties": {
        "Channels": "0",
        "Force": false
      }
    },
    {
      "Id": "03",
      "Type": "PEIS",
      "Properties": {
        "ChannelIndex": 0,
        "InitialFrequency_Hz": 100000,
        "FinalFrequency_Hz": 0.01,
        "FrequencyPoints": 50,
        "DcVoltage_V": 0.0,
        "AcAmplitude_V": 0.01
      }
    },
    {
      "Id": "04",
      "Type": "WaitForCompletion",
      "Properties": {
        "ChannelIndex": 0,
        "TimeoutSeconds": 3600
      }
    },
    {
      "Id": "05",
      "Type": "GetData",
      "Properties": {
        "ChannelIndex": 0,
        "OutputFile": "peis_data.csv"
      }
    },
    {
      "Id": "06",
      "Type": "DisconnectDevice"
    }
  ]
}
```

## Notes

- All channel indices are **zero-based** (0-15 for a 16-channel device)
- The ECLib API internally uses 1-based indexing, but SequenceItems handle the conversion
- Technique files (.ecc) must be present in the `Techniques` folder
- Data files are saved to the `Data` folder by default (relative paths are resolved)
- All SequenceItems follow the Dispenser pattern for consistency
- Error handling returns detailed error messages in the `ResultParameter`

## Common Error Scenarios

1. **Device not connected**: Ensure `ConnectDevice` is called first
2. **Firmware not loaded**: Some techniques require firmware to be loaded to the channel
3. **Technique file not found**: Verify .ecc files are in the Techniques folder
4. **Invalid channel index**: Check that the channel is within the valid range (0-15)
5. **Communication timeout**: Increase timeout values or check device connection
