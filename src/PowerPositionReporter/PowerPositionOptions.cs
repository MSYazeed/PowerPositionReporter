namespace PowerPositionReporter;

/// <summary>
/// Configuration for scheduled power position extracts.
/// </summary>
public sealed class PowerPositionOptions
{
    public const string SectionName = "PowerPosition";

    public string OutputFolder { get; init; } = string.Empty;

    public int IntervalMinutes { get; init; }

    /// <summary>
    /// When true, the worker will continue running and processing future scheduled extracts even if an extract fails.
    /// When false, any exception will terminate the worker service.
    /// Set to true for production resilience against transient errors from external services.
    /// </summary>
    public bool ContinueOnError { get; init; } = true;
}
