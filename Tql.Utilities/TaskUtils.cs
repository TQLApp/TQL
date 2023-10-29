namespace Tql.Utilities;

public static class TaskUtils
{
    public static void RunSynchronously(Func<Task> action)
    {
        Task.Run(action).GetAwaiter().GetResult();
    }

    public static T RunSynchronously<T>(Func<Task<T>> action)
    {
        return Task.Run(action).GetAwaiter().GetResult();
    }

    // See https://stackoverflow.com/questions/5613951 for more information.
    public static void RunBackground(Func<Task> action)
    {
        Task.Run(action).ConfigureAwait(false);
    }
}
