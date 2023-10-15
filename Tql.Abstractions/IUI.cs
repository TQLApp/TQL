namespace Tql.Abstractions;

public interface IUI
{
    Task PerformInteractiveAuthentication(IInteractiveAuthentication interactiveAuthentication);
    void LaunchUrl(string url);
}
