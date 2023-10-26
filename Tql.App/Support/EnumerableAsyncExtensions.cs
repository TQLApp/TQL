using NuGet.Common;

namespace Tql.App.Support;

internal static class EnumerableAsyncExtensions
{
    public static async Task<List<T>> ToListAsync<T>(this IEnumerableAsync<T> self)
    {
        var enumerator = self.GetEnumeratorAsync();

        var result = new List<T>();

        while (await enumerator.MoveNextAsync())
        {
            result.Add(enumerator.Current);
        }

        return result;
    }
}
