namespace Launcher.Abstractions;

public interface IInteractiveAuthentication
{
    string ResourceName { get; }

    Task Authenticate(IWin32Window owner);
}
