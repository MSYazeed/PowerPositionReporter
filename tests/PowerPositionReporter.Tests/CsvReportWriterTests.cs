namespace PowerPositionReporter.Tests;

public sealed class CsvReportWriterTests
{
    [Fact]
    public async Task WriteAsync_WritesExpectedHeaderRowsAndInvariantVolumes()
    {
        var outputFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.csv");

        try
        {
            var rows = new[]
            {
                new PowerPositionRow("23:00", 150),
                new PowerPositionRow("00:00", 80.5),
                new PowerPositionRow("01:00", 1.0 / 3.0)
            };

            await new CsvReportWriter().WriteAsync(outputFilePath, rows, CancellationToken.None);

            var lines = await File.ReadAllLinesAsync(outputFilePath, CancellationToken.None);

            Assert.Equal(
                [
                    "Local Time,Volume",
                    "23:00,150",
                    "00:00,80.5",
                    "01:00,0.33333333333333331"
                ],
                lines);
        }
        finally
        {
            if (File.Exists(outputFilePath))
            {
                File.Delete(outputFilePath);
            }
        }
    }
}
