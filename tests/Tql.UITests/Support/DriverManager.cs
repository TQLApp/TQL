using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace Tql.UITests.Support;

internal class DriverManager
{
    private const string MessageName = "$TQL.IPC^WM$";
    private const string WindowName = "$TQL.IPC$";

    private readonly string _path;
    private readonly uint _windowMessage;
    private readonly string _environment;

    public DriverManager(string environment, string path)
    {
        _environment = environment;
        _path = Environment.ExpandEnvironmentVariables(path);

        _windowMessage = RegisterWindowMessage(GetFullName(MessageName));
    }

    public Session StartApp()
    {
        var options = new AppiumOptions();

        options.AddAdditionalCapability("app", _path);
        options.AddAdditionalCapability(
            "appArguments",
            ShellUtils.PrintArguments("--env", _environment)
        );

        return CreateSession(options, true);
    }

    public void ResetApp()
    {
        using var process = Process.Start(
            _path,
            ShellUtils.PrintArguments("--env", _environment, "--reset", "--silent")
        );

        var exited = process!.WaitForExit((int)Constants.ResetTimeout.TotalMilliseconds);

        if (!exited)
        {
            process.Kill();

            throw new InvalidOperationException(
                $"Reset did not complete within timeout {Constants.ResetTimeout}"
            );
        }
    }

    public Session GetApp(bool takeOwnership)
    {
        var handle = RetryUtils.Retry(GetIPCWindow, TimeSpan.FromSeconds(5));

        return CreateSession(handle, takeOwnership);
    }

    private IntPtr GetIPCWindow()
    {
        var handle = FindWindow(null, GetFullName(WindowName));
        if (handle == IntPtr.Zero)
            throw new RetryException("Cannot find window");

        return handle;
    }

    public Session? FindApp(bool takeOwnership)
    {
        var handle = FindWindow(null, GetFullName(WindowName));
        if (handle == IntPtr.Zero)
            return null;

        return CreateSession(handle, takeOwnership);
    }

    private Session CreateSession(IntPtr handle, bool takeOwnership)
    {
        var options = new AppiumOptions();

        options.AddAdditionalCapability("appTopLevelWindow", handle.ToString("x"));

        return CreateSession(options, takeOwnership);
    }

    public void KillApp()
    {
        RetryUtils.Retry(
            () =>
            {
                foreach (var process in Process.GetProcessesByName(Constants.AppName))
                {
                    process.Kill();
                }
            },
            TimeSpan.FromSeconds(0.5)
        );
    }

    public void KillApp(TimeSpan timeout)
    {
        try
        {
            RetryUtils.Retry(
                () =>
                {
                    if (Process.GetProcessesByName(Constants.AppName).Length > 0)
                        throw new RetryException("Process is still running");
                },
                timeout
            );
        }
        catch (RetryException)
        {
            KillApp();
        }
    }

    private string GetFullName(string name) => $"{name}#{_environment.ToUpperInvariant()}";

    private Session CreateSession(AppiumOptions options, bool isOwner)
    {
        options.AddAdditionalCapability("deviceName", "WindowsPC");

        return new Session(
            new WindowsDriver<WindowsElement>(new Uri(Constants.WinAppDriverEndpoint), options),
            isOwner,
            this
        );
    }

    public void RequestOpenMainWindow()
    {
        var handle = GetIPCWindow();

        PostMessage(handle, _windowMessage, IntPtr.Zero, IntPtr.Zero);
    }

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern uint RegisterWindowMessage(string lpString);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
}
