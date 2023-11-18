namespace Tql.Utilities;

internal static class TaskUtils
{
    public static void RunBackground(Func<Task> action)
    {
        Task.Run(action).ConfigureAwait(false);
    }
}
