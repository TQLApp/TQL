using Microsoft.Win32;

namespace Tql.UITests.Support;

internal class FixtureBase(
    string environment = Constants.DefaultEnvironment,
    string path = Constants.DefaultPath
)
{
    private readonly DriverManager _driverManager = new(environment, path);

    public Session StartApp() => _driverManager.StartApp();

    public Session GetApp(bool takeOwnership) => _driverManager.GetApp(takeOwnership);

    public Session? FindApp(bool takeOwnership) => _driverManager.FindApp(takeOwnership);

    public void ResetApp()
    {
        KillApp();

        _driverManager.ResetApp();

        using var key = Registry.CurrentUser.CreateSubKey($"Software\\TQL - {environment}")!;

        key.SetValue("Language", Constants.DefaultLanguage);
        key.SetValue("QuickStart", Constants.DefaultQuickStartState);
    }

    public void KillApp() => _driverManager.KillApp();
}
