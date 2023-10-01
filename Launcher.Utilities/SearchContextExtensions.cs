using Launcher.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Launcher.Utilities;

public static class SearchContextExtensions
{
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
