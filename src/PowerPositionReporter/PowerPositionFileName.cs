using System.Globalization;

namespace PowerPositionReporter;

public static class PowerPositionFileName
{
    public static string Create(DateTimeOffset extractLocalTime)
    {
        return string.Create(
            CultureInfo.InvariantCulture,
            $"PowerPosition_{extractLocalTime:yyyyMMdd_HHmm}.csv");
    }
}
