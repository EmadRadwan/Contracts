namespace Application.Core;

public static class DateHelper
{
    public static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow.Date);

    // DateTime → DateOnly
    public static DateOnly ToDateOnly(this DateTime dateTime)
    {
        return DateOnly.FromDateTime(dateTime.Date);
    }

    public static DateOnly? ToDateOnly(this DateTime? dateTime)
    {
        return dateTime?.ToDateOnly();
    }

    // DateOnly → DateTime (non-nullable)
    public static DateTime ToDateTime(this DateOnly dateOnly, TimeOnly time = default)
    {
        return dateOnly.ToDateTime(time);
    }

    public static DateTime ToStartOfDay(this DateOnly dateOnly)
    {
        return dateOnly.ToDateTime(TimeOnly.MinValue);
    }

    public static DateTime ToEndOfDay(this DateOnly dateOnly)
    {
        return dateOnly.ToDateTime(TimeOnly.MaxValue);
    }

    // NEW: Support for nullable DateOnly
    public static DateTime? ToDateTime(this DateOnly? dateOnly, TimeOnly time = default)
    {
        return dateOnly?.ToDateTime(time);
    }

    public static DateTime? ToStartOfDay(this DateOnly? dateOnly)
    {
        return dateOnly?.ToDateTime(TimeOnly.MinValue);
    }

    public static DateTime? ToEndOfDay(this DateOnly? dateOnly)
    {
        return dateOnly?.ToDateTime(TimeOnly.MaxValue);
    }
    
    public static bool IsInMonth(this DateOnly? dateOnly, int month, int year)
    {
        return dateOnly.HasValue 
               && dateOnly.Value.Month == month 
               && dateOnly.Value.Year == year;
    }

    public static (int Month, int Year) GetPreviousMonth(this DateOnly date)
    {
        var prevMonth = date.Month == 1 ? 12 : date.Month - 1;
        var prevYear = date.Month == 1 ? date.Year - 1 : date.Year;
        return (prevMonth, prevYear);
    }

    public static (int Month, int Year) GetPreviousMonth(this DateOnly? date)
    {
        return date?.GetPreviousMonth() ?? (DateHelper.Today.Month, DateHelper.Today.Year);
    }
}