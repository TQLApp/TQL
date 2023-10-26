namespace Tql.App.Support;

internal static class CollectionExtensions
{
    public static void AddRange<T>(this ICollection<T> self, IEnumerable<T> values)
    {
        foreach (var value in values)
        {
            self.Add(value);
        }
    }
}
