using System.Globalization;
using System.Text;

namespace PowerPositionReporter;

/// <summary>
/// Writes power position rows in the CSV format required by the challenge.
/// </summary>
public sealed class CsvReportWriter
{
    public async Task WriteAsync(
        string outputFilePath,
        IEnumerable<PowerPositionRow> rows,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputFilePath);
        ArgumentNullException.ThrowIfNull(rows);

        await using var stream = new FileStream(
            outputFilePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true);

        await using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        await writer.WriteLineAsync("Local Time,Volume".AsMemory(), cancellationToken);

        foreach (var row in rows)
        {
            var volume = row.Volume.ToString("G17", CultureInfo.InvariantCulture);
            var line = $"{row.LocalTime},{volume}";

            await writer.WriteLineAsync(line.AsMemory(), cancellationToken);
        }
    }
}
