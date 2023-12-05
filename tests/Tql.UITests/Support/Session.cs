using System.Diagnostics;
using System.Windows.Ink;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;

namespace Tql.UITests.Support;

internal class Session(
    WindowsDriver<WindowsElement> driver,
    bool isOwner,
    DriverManager driverManager
) : IDisposable
{
    public WindowsDriver<WindowsElement> Driver { get; } = driver;

    public void OpenMainWindow() => OpenMainWindow(Constants.OpenMainWindowTimeout);

    public void OpenMainWindow(TimeSpan timeout) =>
        RetryUtils.Retry(
            () =>
            {
                driverManager.RequestOpenMainWindow();

                if (!SwitchToWindow(Id.MainWindow))
                    throw new RetryException("Main window did not open");
            },
            timeout
        );

    private bool SwitchToWindow(string automationId)
    {
        foreach (var handle in Driver.WindowHandles)
        {
            Driver.SwitchTo().Window(handle);

            if (Driver.HasElementByAccessibilityId(automationId))
                return true;
        }

        return false;
    }

    public void CloseOpenWindows()
    {
        RetryUtils.Retry(
            () =>
            {
                if (Driver.WindowHandles.Count == 0)
                    return;

                foreach (var element in Driver.FindElementsByXPath("//Window"))
                {
                    element.SendKeys(Keys.Alt + Keys.F4 + Keys.Alt);
                }

                throw new RetryException("There are still windows open");
            },
            Constants.CloseOpenWindowsTimeout
        );
    }

    public void WaitForSingleWindow() => WaitForSingleWindow(TimeSpan.FromSeconds(5));

    public void WaitForSingleWindow(TimeSpan timeout) =>
        RetryUtils.Retry(
            () =>
            {
                var handles = Driver.WindowHandles.ToList();
                if (handles.Count != 1)
                    throw new RetryException("Expected a single window");

                Driver.SwitchTo().Window(handles[0]);
            },
            timeout
        );

    public void Dispose()
    {
        if (isOwner)
            driverManager.KillApp();

        Driver.Dispose();
    }

    public void OpenConfigurationWindow()
    {
        OpenMainWindow();

        ClickButton(Id.ConfigurationButton);

        RetryUtils.Retry(
            () =>
            {
                if (!SwitchToWindow(Id.ConfigurationWindow))
                    throw new RetryException("Configuration window is not visible");
            },
            Constants.OpenConfigurationWindowTimeout
        );
    }

    private void ClickButton(string automationId)
    {
        Driver.FindElementByAccessibilityId(automationId).Click();
    }
}
