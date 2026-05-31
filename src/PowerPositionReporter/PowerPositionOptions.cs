namespace PowerPositionReporter;

/// <summary>
/// Configuration for scheduled power position extracts.
/// </summary>
public sealed class PowerPositionOptions
{
    public const string SectionName = "PowerPosition";

    public string OutputFolder { get; init; } = string.Empty;

    public int IntervalMinutes { get; init; }
}
