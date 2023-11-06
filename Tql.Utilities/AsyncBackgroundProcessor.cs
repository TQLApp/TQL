using Microsoft.Extensions.Logging;

namespace Tql.Utilities;

internal class AsyncBackgroundProcessor
{
    private readonly ILogger _logger;
    private readonly object _syncRoot = new();
    private readonly Queue<Func<Task>> _queue = new();
    private bool _running;

    public AsyncBackgroundProcessor(ILogger logger)
    {
        _logger = logger;
    }

    public void Enqueue(Func<Task> task)
    {
        bool run;

        lock (_syncRoot)
        {
            _queue.Enqueue(task);
            run = !_running;
            _running = true;
        }

        if (run)
        {
            TaskUtils.RunBackground(ProcessQueue);
        }
    }

    private async Task ProcessQueue()
    {
        while (true)
        {
            Func<Task> task;

            lock (_syncRoot)
            {
                if (_queue.Count == 0)
                {
                    _running = false;
                    return;
                }

                task = _queue.Dequeue();
            }

            try
            {
                await task();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while executing queued task");
            }
        }
    }
}
