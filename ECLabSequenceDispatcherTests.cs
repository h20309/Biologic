using Biologic;
using Biologic.Native;
using FluentAssertions;

namespace Biologic.Tests;

/// <summary>
/// Tests for ECLabSequenceDispatcher and ChannelDataBatch
/// </summary>
public class ECLabSequenceDispatcherTests
{
  [Fact]
  public void ECLabSequenceDispatcher_ShouldInitializeWithDefaultValues()
  {
    // Arrange & Act
    using var dispatcher = new ECLabSequenceDispatcher();

    // Assert
    dispatcher.IsPollingEnabled.Should().BeFalse();
    dispatcher.DefaultPollingInterval.Should().Be(1000);
  }

  [Fact]
  public void IsPollingEnabled_WhenSet_ShouldUpdateValue()
  {
    // Arrange
    using var dispatcher = new ECLabSequenceDispatcher();

    // Act
    dispatcher.IsPollingEnabled = true;

    // Assert
    dispatcher.IsPollingEnabled.Should().BeTrue();
  }

  [Fact]
  public void DefaultPollingInterval_WhenSet_ShouldUpdateValue()
  {
    // Arrange
    using var dispatcher = new ECLabSequenceDispatcher();

    // Act
    dispatcher.DefaultPollingInterval = 500;

    // Assert
    dispatcher.DefaultPollingInterval.Should().Be(500);
  }

  [Fact]
  public void Dispose_ShouldCleanupResources()
  {
    // Arrange
    var dispatcher = new ECLabSequenceDispatcher();

    // Act
    Action act = () => dispatcher.Dispose();

    // Assert
    act.Should().NotThrow();
  }

  [Fact]
  public void GetLatestData_WithoutPolling_ShouldReturnNull()
  {
    // Arrange
    using var dispatcher = new ECLabSequenceDispatcher();

    // Act
    var data = dispatcher.GetLatestData(0);

    // Assert
    data.Should().BeNull();
  }

  [Fact]
  public void GetAllData_WithoutPolling_ShouldReturnEmptyList()
  {
    // Arrange
    using var dispatcher = new ECLabSequenceDispatcher();

    // Act
    var allData = dispatcher.GetAllData(0);

    // Assert
    allData.Should().BeEmpty();
  }

  [Fact]
  public void ClearData_WithoutPolling_ShouldNotThrow()
  {
    // Arrange
    using var dispatcher = new ECLabSequenceDispatcher();

    // Act
    Action act = () => dispatcher.ClearData(0);

    // Assert
    act.Should().NotThrow();
  }
}

/// <summary>
/// Tests for ChannelDataBatch
/// </summary>
public class ChannelDataBatchTests
{
  [Fact]
  public void ChannelDataBatch_ShouldInitializeWithDefaultValues()
  {
    // Arrange & Act
    var batch = new ChannelDataBatch();

    // Assert
    batch.Channel.Should().Be(0);
    batch.DataBuffer.Should().BeEmpty();
    batch.TechniqueId.Should().Be(TechniqueId.None);
  }

  [Fact]
  public void ChannelDataBatch_ShouldAllowPropertyAssignment()
  {
    // Arrange
    var batch = new ChannelDataBatch
    {
      Channel = 5,
      TechniqueId = TechniqueId.CV,
      Timestamp = DateTime.Now
    };

    // Act & Assert
    batch.Channel.Should().Be(5);
    batch.TechniqueId.Should().Be(TechniqueId.CV);
    batch.Timestamp.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
  }

  [Fact]
  public void GetColumnAsFloats_WithInvalidColumnIndex_ShouldThrowArgumentOutOfRangeException()
  {
    // Arrange
    var batch = new ChannelDataBatch
    {
      DataInfo = new DataInfo { NbRows = 10, NbCols = 5 },
      DataBuffer = new uint[50]
    };

    // Act
    Action act = () => batch.GetColumnAsFloats(10);

    // Assert
    act.Should().Throw<ArgumentOutOfRangeException>();
  }

  [Fact]
  public void GetColumnAsFloats_WithValidColumnIndex_ShouldReturnFloatArray()
  {
    // Arrange
    var batch = new ChannelDataBatch
    {
      DataInfo = new DataInfo { NbRows = 3, NbCols = 2 },
      DataBuffer = new uint[] { 0, 1, 2, 3, 4, 5 }
    };

    // Act
    var column = batch.GetColumnAsFloats(0);

    // Assert
    column.Should().HaveCount(3);
  }

  [Fact]
  public void GetTimeColumn_WithValidData_ShouldReturnTimeArray()
  {
    // Arrange
    var batch = new ChannelDataBatch
    {
      DataInfo = new DataInfo { NbRows = 5, NbCols = 3 },
      DataBuffer = new uint[15],
      CurrentValues = new CurrentValues
      {
        ElapsedTime = 10.0f,
        TimeBase = 0.1f
      }
    };

    // Act
    var timeColumn = batch.GetTimeColumn();

    // Assert
    timeColumn.Should().HaveCount(5);
    timeColumn[0].Should().BeApproximately(10.0, 0.01);
  }

  [Fact]
  public void ToCsv_ShouldReturnFormattedCsvString()
  {
    // Arrange
    var batch = new ChannelDataBatch
    {
      Channel = 0,
      TechniqueId = TechniqueId.OCV,
      Timestamp = new DateTime(2024, 1, 1, 12, 0, 0),
      DataInfo = new DataInfo { NbRows = 2, NbCols = 2 },
      DataBuffer = new uint[] { 0, 0, 0, 0 }
    };

    // Act
    var csv = batch.ToCsv();

    // Assert
    csv.Should().Contain("# Channel: 0");
    csv.Should().Contain("# Technique: OCV");
    csv.Should().Contain("# Timestamp: 2024-01-01 12:00:00");
    csv.Should().Contain("# Rows: 2, Cols: 2");
  }

  [Fact]
  public void ToCsv_WithEmptyData_ShouldReturnHeaderOnly()
  {
    // Arrange
    var batch = new ChannelDataBatch
    {
      Channel = 1,
      TechniqueId = TechniqueId.CV,
      DataInfo = new DataInfo { NbRows = 0, NbCols = 0 }
    };

    // Act
    var csv = batch.ToCsv();

    // Assert
    csv.Should().Contain("# Channel: 1");
    csv.Should().Contain("# Technique: CV");
    csv.Should().Contain("# Rows: 0, Cols: 0");
  }
}
