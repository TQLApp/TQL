namespace Tql.App.Support;

internal static class StringExtensions
{
    public static int GetStableHashCode(this string self)
    {
        var hashCode = 0;

        foreach (var c in self)
        {
            hashCode = (hashCode * 397) ^ (int)c;
        }

        return hashCode;
    }
}
