namespace Launcher.Abstractions;

public interface ICopyableMatch : IMatch
{
    Task Copy(IServiceProvider serviceProvider);
}
