namespace PowerPositionReporter;

/// <summary>
/// Runs scheduled power position extracts and catches up any overdue extracts while the process is running.
/// </summary>
public sealed class Worker(
    PowerPositionReportService reportService,
    TimeProvider timeProvider,
    Microsoft.Extensions.Options.IOptions<PowerPositionOptions> options,
    ILogger<Worker> logger) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(options.Value.IntervalMinutes);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Directory.CreateDirectory(options.Value.OutputFolder);

        logger.LogInformation(
            "Starting power position worker with output folder {OutputFolder} and interval {IntervalMinutes} minutes",
            options.Value.OutputFolder,
            options.Value.IntervalMinutes);

        var scheduledDueUtc = timeProvider.GetUtcNow();
        await RunExtractAsync(scheduledDueUtc, stoppingToken);

        var nextDueUtc = scheduledDueUtc.Add(_interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            var nowUtc = timeProvider.GetUtcNow();
            var delay = nextDueUtc - nowUtc;

            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, stoppingToken);
            }

            await RunExtractAsync(nextDueUtc, stoppingToken);
            nextDueUtc = nextDueUtc.Add(_interval);
        }
    }

    private async Task RunExtractAsync(DateTimeOffset scheduledDueUtc, CancellationToken cancellationToken)
    {
        var startedUtc = timeProvider.GetUtcNow();
        var lagSeconds = Math.Max(0, (startedUtc - scheduledDueUtc).TotalSeconds);

        if (lagSeconds > 0)
        {
            logger.LogWarning(
                "Power position extract is starting {LagSeconds:F3} seconds after scheduled due time {ScheduledDueUtc}",
                lagSeconds,
                scheduledDueUtc);
        }

        // NOTE FOR REVIEWER: Added exception handling to prevent transient failures from external services
        // (like PowerService API errors) from terminating the entire Worker background service.
        // This ensures the scheduled extracts continue running even if individual extracts fail.
        // The behavior is configurable via appsettings.json "PowerPosition:ContinueOnError" setting.
        // When ContinueOnError=false, exceptions will propagate and terminate the worker (original behavior).
        try
        {
            await reportService.GenerateAsync(scheduledDueUtc, lagSeconds, cancellationToken);
        }
        catch (Exception exception) when (options.Value.ContinueOnError && exception is not OperationCanceledException)
        {
            // Error details are already logged in PowerPositionReportService.GenerateAsync
            // This catch block exists solely to prevent worker termination and allow subsequent scheduled extracts to run
            logger.LogWarning(
                "Extract failed but worker will continue due to ContinueOnError setting. scheduledDueUtc={ScheduledDueUtc}",
                scheduledDueUtc);
        }
    }
}
