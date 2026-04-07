namespace Application.Core;

public static class DateHelper
{
    /// <summary>
    /// Normalizes any DateTime to date-only (midnight) in a timezone-safe way.
    /// </summary>
    public static DateTime ToDateOnly(this DateTime dateTime)
    {
        if (dateTime == default) 
            return DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Unspecified);

        // Strip time and force Unspecified kind to avoid timezone conversion issues
        return DateTime.SpecifyKind(dateTime.Date, DateTimeKind.Unspecified);
    }

    /// <summary>
    /// Gets today's date in a consistent way (UTC date only)
    /// </summary>
    public static DateTime Today => DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Unspecified);
}