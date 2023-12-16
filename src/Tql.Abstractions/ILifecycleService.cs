namespace Tql.Abstractions;

/// <summary>
/// Represents a service that handles lifecycle events.
/// </summary>
/// <remarks>
/// This service does not expose hooks for host start and stop
/// events. Use <see cref="Microsoft.Extensions.Hosting.IHostedService"/>
/// for those events instead.
/// </remarks>
public interface ILifecycleService
{
    /// <summary>
    /// Register a callback to be run after the host shuts down.
    /// </summary>
    /// <param name="action">Callback to be called.</param>
    void RegisterAfterHostTermination(Action action);

    /// <summary>
    /// Register a callback to be run before the app terminates.
    /// </summary>
    /// <param name="action">Callback to be called.</param>
    void RegisterBeforeShutdown(Action action);
}
