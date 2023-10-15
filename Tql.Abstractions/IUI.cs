namespace Tql.Abstractions;

public interface IUI
{
    Task PerformInteractiveAuthentication(IInteractiveAuthentication interactiveAuthentication);
    void OpenUrl(string url);
}
