using Axpo;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace PowerPositionReporter;

/// <summary>
/// Coordinates retrieval, aggregation, and CSV writing for one scheduled power position extract.
/// </summary>
public sealed class PowerPositionReportService(
    IPowerService powerService,
    PowerPositionAggregator aggregator,
    CsvReportWriter csvReportWriter,
    TimeZoneInfo londonTimeZone,
    IOptions<PowerPositionOptions> options,
    ILogger<PowerPositionReportService> logger)
{
    public async Task GenerateAsync(
        DateTimeOffset scheduledDueUtc,
        double lagSeconds,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var extractLocalTime = TimeZoneInfo.ConvertTime(scheduledDueUtc, londonTimeZone);
        var reportDate = extractLocalTime.Date.AddDays(1);
        var outputFilePath = Path.Combine(options.Value.OutputFolder, PowerPositionFileName.Create(extractLocalTime));

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var trades = (await powerService.GetTradesAsync(reportDate)).ToArray();

            cancellationToken.ThrowIfCancellationRequested();

            var rows = aggregator.Aggregate(trades);
            var periodCount = trades.Sum(trade => trade.Periods.Length);

            await csvReportWriter.WriteAsync(outputFilePath, rows, cancellationToken);

            stopwatch.Stop();

            logger.LogInformation(
                "Generated power position extract. scheduledDueUtc={ScheduledDueUtc}, extractLocalTime={ExtractLocalTime}, reportDate={ReportDate}, outputFilePath={OutputFilePath}, duration={Duration}, lagSeconds={LagSeconds:F3}, tradeCount={TradeCount}, periodCount={PeriodCount}",
                scheduledDueUtc,
                extractLocalTime,
                reportDate,
                outputFilePath,
                stopwatch.Elapsed,
                lagSeconds,
                trades.Length,
                periodCount);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            stopwatch.Stop();

            logger.LogError(
                exception,
                "Failed to generate power position extract. scheduledDueUtc={ScheduledDueUtc}, extractLocalTime={ExtractLocalTime}, reportDate={ReportDate}, outputFilePath={OutputFilePath}, duration={Duration}, lagSeconds={LagSeconds:F3}",
                scheduledDueUtc,
                extractLocalTime,
                reportDate,
                outputFilePath,
                stopwatch.Elapsed,
                lagSeconds);

            throw;
        }
    }
}
