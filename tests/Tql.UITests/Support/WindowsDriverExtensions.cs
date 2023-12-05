using OpenQA.Selenium.Appium.Windows;

namespace Tql.UITests.Support;

internal static class WindowsDriverExtensions
{
    public static bool HasElementByAccessibilityId(
        this WindowsDriver<WindowsElement> self,
        string selector
    ) => self.FindElementsByAccessibilityId(selector).Any();
}
