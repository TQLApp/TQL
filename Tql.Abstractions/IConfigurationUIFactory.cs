namespace Tql.Abstractions;

public interface IConfigurationUIFactory
{
    Guid Id { get; }
    string Title { get; }

    public IConfigurationUI CreateControl(IServiceProvider serviceProvider);
}
