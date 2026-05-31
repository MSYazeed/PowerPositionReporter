namespace PowerPositionReporter.Tests;

public sealed class PowerPositionFileNameTests
{
    [Fact]
    public void Create_UsesScheduledExtractLocalTime()
    {
        var scheduledExtractLocalTime = new DateTimeOffset(2026, 5, 31, 5, 23, 45, TimeSpan.FromHours(1));

        var fileName = PowerPositionFileName.Create(scheduledExtractLocalTime);

        Assert.Equal("PowerPosition_20260531_0523.csv", fileName);
    }
}
