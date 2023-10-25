namespace Tql.App.Services;

internal class ConfigurationUIEventArgs : EventArgs
{
    public Guid Id { get; }

    public ConfigurationUIEventArgs(Guid id)
    {
        Id = id;
    }
}
