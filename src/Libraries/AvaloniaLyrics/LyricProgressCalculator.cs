using System;

namespace AvaloniaLyrics;

public static class LyricProgressCalculator
{
    public static double GetProgress(TimeSpan position, TimeSpan start, TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
            return position >= start ? 1d : 0d;

        var value = (position - start).TotalMilliseconds / duration.TotalMilliseconds;
        return Math.Clamp(value, 0d, 1d);
    }

    public static bool IsCurrent(double progress)
    {
        return progress > 0d && progress < 1d;
    }

    public static bool IsPlayed(double progress)
    {
        return progress >= 1d;
    }

    public static double GetLiftOffset(double progress, double amplitude = 8d)
    {
        if (!IsCurrent(progress))
            return 0d;

        return -Math.Sin(progress * Math.PI) * amplitude;
    }
}
