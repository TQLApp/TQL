namespace Tql.Plugins.Confluence.Support;

public static class TaskUtils
{
    // See https://stackoverflow.com/questions/5613951 for more information.
    public static void RunBackground(Func<Task> action)
    {
        Task.Run(action).ConfigureAwait(false);
    }
}
