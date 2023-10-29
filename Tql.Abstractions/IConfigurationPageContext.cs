namespace Tql.Abstractions;

public interface IConfigurationPageContext
{
    bool IsVisible { get; }

    event EventHandler? IsVisibleChanged;
    event EventHandler? Closed;
}
