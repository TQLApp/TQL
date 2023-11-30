namespace Tql.App.Support;

internal static class WpfUtils
{
    public static double PointsToPixels(int points)
    {
        return (points / 72.0) * 96.0;
    }
}
