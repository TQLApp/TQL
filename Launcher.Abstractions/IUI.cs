namespace Launcher.Abstractions;

public interface IUI
{
    Task RunOnAuthenticationThread(Action<IWin32Window> func);
    Task<T> RunOnAuthenticationThread<T>(Func<IWin32Window, T> func);
    void LaunchUrl(string url);
}
