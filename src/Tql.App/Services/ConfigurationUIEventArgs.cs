namespace Tql.App.Services;

internal class ConfigurationUIEventArgs(Guid id) : EventArgs
{
    public Guid Id { get; } = id;
}
