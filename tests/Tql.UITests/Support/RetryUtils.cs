using System.Diagnostics;

namespace Tql.UITests.Support;

internal static class RetryUtils
{
    public static void Retry(Action action, TimeSpan period)
    {
        Retry(action, period, TimeSpan.FromMilliseconds(50));
    }

    public static void Retry(Action action, TimeSpan period, TimeSpan delay)
    {
        var stopwatch = Stopwatch.StartNew();

        while (true)
        {
            try
            {
                action();
                return;
            }
            catch when (stopwatch.Elapsed < period)
            {
                Thread.Sleep(delay);
            }
        }
    }

    public static T Retry<T>(Func<T> action, TimeSpan period)
    {
        return Retry<T>(action, period, TimeSpan.FromMilliseconds(50));
    }

    public static T Retry<T>(Func<T> action, TimeSpan period, TimeSpan delay)
    {
        var stopwatch = Stopwatch.StartNew();

        while (true)
        {
            try
            {
                return action();
            }
            catch when (stopwatch.Elapsed < period)
            {
                Thread.Sleep(delay);
            }
        }
    }
}
