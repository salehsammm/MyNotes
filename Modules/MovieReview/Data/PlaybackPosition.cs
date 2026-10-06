using System.Globalization;

namespace MovieReviews.Data;

public static class PlaybackPosition
{
    public static string Format(int seconds)
    {
        var time = TimeSpan.FromSeconds(seconds);
        return time.TotalHours >= 1
            ? $"{(int)time.TotalHours}:{time.Minutes:00}:{time.Seconds:00}"
            : $"{(int)time.TotalMinutes}:{time.Seconds:00}";
    }

    public static bool TryParse(string? input, out int? seconds)
    {
        seconds = null;
        if (string.IsNullOrWhiteSpace(input)) return true;
        var parts = input.Trim().Split(':');
        if (parts.Length is not (2 or 3)) return false;
        var values = new int[parts.Length];
        for (var i = 0; i < parts.Length; i++)
        {
            if (!int.TryParse(parts[i], NumberStyles.None, CultureInfo.InvariantCulture, out values[i])) return false;
        }
        if (values[^1] >= 60 || (parts.Length == 3 && values[1] >= 60)) return false;
        var total = parts.Length == 2
            ? (long)values[0] * 60 + values[1]
            : (long)values[0] * 3600 + values[1] * 60 + values[2];
        if (total > int.MaxValue) return false;
        seconds = (int)total;
        return true;
    }
}
