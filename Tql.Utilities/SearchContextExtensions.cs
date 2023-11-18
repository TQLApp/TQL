using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tql.Abstractions;

namespace Tql.Utilities;

/// <summary>
/// Utility methods for working with <see cref="ISearchContext"/>'s.
/// </summary>
public static class SearchContextExtensions
{
    /// <summary>
    /// Gets cached data.
    /// </summary>
    /// <remarks>
    /// This method is used to manage short lived caches. These caches
    /// are stored in the <see cref="ISearchContext.Context"/> map
    /// and only persist as long as the user has the search window open.
    ///
    /// This method combines building the cache and retrieving the cache
    /// in one. The first time this method is called, the action is
    /// used to create a task instance, which is stored in the context
    /// map. On subsequent calls, this same task is returned to the caller.
    ///
    /// The intended use of this method is as follows:
    ///
    /// <code><![CDATA[
    /// public async Task<IEnumerable<IMatch>> Search(ISearchContext context, string text, CancellationToken cancellationToken)
    /// {
    ///     // The LoadData method in this examples loads the data from the server.
    ///     // The cache key is a combination of the name of the current class
    ///     // and a string serialized version of the DTO object. This example
    ///     // assumes that the DTO object is a "record".
    ///     //
    ///     // Note that the cancellation token is NOT passed to the LoadData method.
    ///     // If it were, and the first search were cancelled (because the user
    ///     // typed more characters), it would also cancel seeding of the cache.
    ///     var cache = context.GetDataCached($"{GetType().FullName}:{_dto}", LoadData);
    ///
    ///     // It's only necessary to introduce a debounce delay if the cache
    ///     // hasn't completed loading yet.
    ///     if (!cache.IsCompleted)
    ///         await context.DebounceDelay(cancellationToken);
    ///
    ///     // Return a filtered version of the cached matches.
    ///     return await context.Filter(await cache);
    /// }
    /// ]]></code>
    /// </remarks>
    /// <typeparam name="T">Type of the cached data.</typeparam>
    /// <param name="self">Search context to cache data in.</param>
    /// <param name="cacheKey">Cache key. This needs to be globally unique.</param>
    /// <param name="action">Reference to the method that loads the cache.</param>
    /// <returns>Cached data.</returns>
    public static Task<T> GetDataCached<T>(
        this ISearchContext self,
        string cacheKey,
        Func<IServiceProvider, Task<T>> action
    )
    {
        var logger = self.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<T>();

        if (self.Context.TryGetValue(cacheKey, out var value))
            return (Task<T>)value;

        var tcs = new TaskCompletionSource<T>();

        Task.Run(async () =>
        {
            logger.LogInformation("Rebuilding cache for '{Type}'", typeof(T).FullName);

            try
            {
                tcs.SetResult(await action(self.ServiceProvider));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to rebuild cache for '{Type}'", typeof(T).FullName);

                tcs.SetException(ex);
            }
        });

        self.Context[cacheKey] = tcs.Task;

        return tcs.Task;
    }
}
