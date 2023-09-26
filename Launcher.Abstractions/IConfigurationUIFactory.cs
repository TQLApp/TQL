namespace Launcher.Abstractions;

public interface IConfigurationUIFactory
{
    string Title { get; }

    public IConfigurationUI CreateControl(IServiceProvider serviceProvider);
}
