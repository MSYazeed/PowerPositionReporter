namespace PowerPositionReporter;

/// <summary>
/// Resolves the London time zone on Linux and Windows hosts.
/// </summary>
public static class LondonTimeZone
{
    public static TimeZoneInfo GetLondonTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/London");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        }
    }
}
