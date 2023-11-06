namespace Tql.Abstractions;

public interface IInteractiveAuthentication
{
    string ResourceName { get; }

    Task Authenticate(IWin32Window owner);
}
