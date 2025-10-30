namespace RAI.Lab3.Application.Helpers;

public static class TimeZoneHelper
{
    public enum Ambiguous
    {
        Earlier,
        Later
    }

    public static DateTime ToUtcFromZone(DateTime localWallClock, string ianaZone = "Europe/Warsaw",
        Ambiguous ambiguous = Ambiguous.Earlier)
    {
        var tz = GetTimeZone(ianaZone);
        var local = DateTime.SpecifyKind(localWallClock, DateTimeKind.Unspecified);

        if (tz.IsInvalidTime(local))
            throw new ArgumentException($"Local time {local} is invalid in {tz.Id} (DST gap).");

        if (tz.IsAmbiguousTime(local))
        {
            var offsets = tz.GetAmbiguousTimeOffsets(local);
            var chosen = ambiguous == Ambiguous.Earlier ? offsets.Min() : offsets.Max();
            return new DateTimeOffset(local, chosen).UtcDateTime;
        }

        return TimeZoneInfo.ConvertTimeToUtc(local, tz);
    }

    public static DateTime ToZoneFromUtc(DateTime utcInstant, string ianaZone = "Europe/Warsaw")
    {
        var tz = GetTimeZone(ianaZone);
        var utc = utcInstant.Kind == DateTimeKind.Utc
            ? utcInstant
            : DateTime.SpecifyKind(utcInstant, DateTimeKind.Utc);

        return TimeZoneInfo.ConvertTimeFromUtc(utc, tz);
    }

    private static TimeZoneInfo GetTimeZone(string iana)
    {
        return TimeZoneInfo.FindSystemTimeZoneById(iana);
    }
}