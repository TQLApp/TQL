namespace Tql.App.Support;

internal static class TaskUtils
{
    public static void RunSynchronously(Func<Task> action)
    {
        Task.Run(action).GetAwaiter().GetResult();
    }

    public static T RunSynchronously<T>(Func<Task<T>> action)
    {
        return Task.Run(action).GetAwaiter().GetResult();
    }
}
