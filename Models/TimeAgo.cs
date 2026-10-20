namespace CURSOVA.Models;

public static class TimeAgo
{
    public static string Format(DateTime utc, DateTime? nowUtc = null)
    {
        var elapsed = (nowUtc ?? DateTime.UtcNow) - utc;

        if (elapsed < TimeSpan.FromMinutes(1))
            return "щойно";
        if (elapsed < TimeSpan.FromHours(1))
            return UkrainianPlural.Format((int)elapsed.TotalMinutes, "хвилину", "хвилини", "хвилин") + " тому";
        if (elapsed < TimeSpan.FromDays(1))
            return UkrainianPlural.Format((int)elapsed.TotalHours, "годину", "години", "годин") + " тому";
        if (elapsed < TimeSpan.FromDays(2))
            return "вчора";

        return utc.ToLocalTime().ToString("d MMMM");
    }
}
