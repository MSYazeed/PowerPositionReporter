using Axpo;
using System.Globalization;

namespace PowerPositionReporter;

/// <summary>
/// Aggregates vendor power trades into the fixed 24 wall-clock period labels required by the report.
/// </summary>
public sealed class PowerPositionAggregator
{
    private const int PeriodsPerDay = 24;

    public IReadOnlyList<PowerPositionRow> Aggregate(IEnumerable<PowerTrade> trades)
    {
        ArgumentNullException.ThrowIfNull(trades);

        var volumesByPeriod = new double[PeriodsPerDay];

        foreach (var trade in trades)
        {
            foreach (var period in trade.Periods)
            {
                if (period.Period is < 1 or > PeriodsPerDay)
                {
                    throw new InvalidOperationException($"Unsupported power period {period.Period}. Expected periods 1 to 24.");
                }

                volumesByPeriod[period.Period - 1] += period.Volume;
            }
        }

        var rows = new PowerPositionRow[PeriodsPerDay];

        for (var periodNumber = 1; periodNumber <= PeriodsPerDay; periodNumber++)
        {
            // Period labels are fixed by the challenge; avoid DateTime arithmetic around DST transitions.
            var hour = (periodNumber + 22) % PeriodsPerDay;
            var localTime = string.Create(
                CultureInfo.InvariantCulture,
                $"{hour:00}:00");

            rows[periodNumber - 1] = new PowerPositionRow(localTime, volumesByPeriod[periodNumber - 1]);
        }

        return rows;
    }
}
