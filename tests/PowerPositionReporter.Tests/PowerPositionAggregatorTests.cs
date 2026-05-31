using Axpo;

namespace PowerPositionReporter.Tests;

public sealed class PowerPositionAggregatorTests
{
    [Fact]
    public void Aggregate_SumsVolumesAcrossTrades()
    {
        var trade1 = CreateTrade(new Dictionary<int, double>
        {
            [1] = 100,
            [12] = 100,
            [24] = 100
        });
        var trade2 = CreateTrade(new Dictionary<int, double>
        {
            [1] = 50,
            [12] = -20,
            [24] = -20
        });

        var rows = new PowerPositionAggregator().Aggregate([trade1, trade2]);

        Assert.Equal(150, rows[0].Volume);
        Assert.Equal(80, rows[11].Volume);
        Assert.Equal(80, rows[23].Volume);
    }

    [Fact]
    public void Aggregate_UsesFixedPeriodToLocalTimeMapping()
    {
        var rows = new PowerPositionAggregator().Aggregate([]);

        Assert.Equal("23:00", rows[0].LocalTime);
        Assert.Equal("00:00", rows[1].LocalTime);
        Assert.Equal("10:00", rows[11].LocalTime);
        Assert.Equal("22:00", rows[23].LocalTime);
    }

    [Fact]
    public void Aggregate_ReturnsStableTwentyFourRowOrdering()
    {
        var rows = new PowerPositionAggregator().Aggregate([]);

        Assert.Equal(24, rows.Count);
        Assert.Equal(
            [
                "23:00",
                "00:00",
                "01:00",
                "02:00",
                "03:00",
                "04:00",
                "05:00",
                "06:00",
                "07:00",
                "08:00",
                "09:00",
                "10:00",
                "11:00",
                "12:00",
                "13:00",
                "14:00",
                "15:00",
                "16:00",
                "17:00",
                "18:00",
                "19:00",
                "20:00",
                "21:00",
                "22:00"
            ],
            rows.Select(row => row.LocalTime).ToArray());
    }

    private static PowerTrade CreateTrade(IReadOnlyDictionary<int, double> volumesByPeriod)
    {
        var trade = PowerTrade.Create(new DateTime(2026, 6, 1), 24);

        var periods = trade.Periods;

        for (var i = 0; i < periods.Length; i++)
        {
            var period = periods[i];

            if (volumesByPeriod.TryGetValue(period.Period, out var volume))
            {
                period.SetVolume(volume);
                periods[i] = period;
            }
        }

        return trade;
    }
}
