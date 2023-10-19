using System.Globalization;
using System.Windows.Forms;
using Tql.Abstractions;
using Tql.App.Services;
using Tql.App.Support;

namespace Tql.App.ConfigurationUI;

internal partial class GeneralConfigurationControl : IConfigurationUI
{
    private readonly Settings _settings;
    private readonly ShowOnScreenManager _showOnScreenManager;

    public GeneralConfigurationControl(Settings settings)
    {
        _settings = settings;

        InitializeComponent();

        _showOnScreenManager = ShowOnScreenManager.Create(settings.ShowOnScreen);

        _showOnScreen.ItemsSource = Enumerable.Range(1, Screen.AllScreens.Length).ToList();
        _showOnScreen.SelectedIndex = _showOnScreenManager.GetScreenIndex();

        _historyInRootResults.ConfigureAsNumericOnly(NumberStyles.None, false);
        _historyInRootResults.Text = settings.HistoryInRootResults?.ToString();

        _cacheUpdateInterval.ConfigureAsNumericOnly(NumberStyles.None, false);
        _cacheUpdateInterval.Text = settings.CacheUpdateInterval?.ToString();

        _mainFontSize.ConfigureAsNumericOnly(NumberStyles.None, false);
        _mainFontSize.Text = settings.MainFontSize?.ToString();

        _mainWindowTint.SelectedColor = BlurWindow.ParseMainWindowTint(
            settings.MainWindowTint ?? Settings.DefaultMainWindowTint
        );

        _theme.ItemsSource = Enum.GetValues(typeof(Theme));
        _theme.SelectedValue = ThemeManager.ParseTheme(settings.Theme);
    }

    public SaveStatus Save()
    {
        if (int.TryParse(_historyInRootResults.Text, out int historyInRootResults))
            _settings.HistoryInRootResults = historyInRootResults;
        if (int.TryParse(_cacheUpdateInterval.Text, out int cacheUpdateInterval))
            _settings.CacheUpdateInterval = cacheUpdateInterval;
        if (int.TryParse(_mainFontSize.Text, out int mainFontSize))
            _settings.MainFontSize = mainFontSize;

        _showOnScreenManager.SetScreenIndex(_showOnScreen.SelectedIndex);

        _settings.ShowOnScreen = _showOnScreenManager.ToString();

        var mainWindowTint = _mainWindowTint.SelectedColor.HasValue
            ? BlurWindow.PrintMainWindowTint(_mainWindowTint.SelectedColor.Value)
            : null;

        if (mainWindowTint == Settings.DefaultMainWindowTint)
            mainWindowTint = null;

        _settings.MainWindowTint = mainWindowTint;

        var theme = (Theme)_theme.SelectedValue;
        _settings.Theme = theme == Theme.System ? null : theme.ToString();

        return SaveStatus.Success;
    }

    private void _mainWindowTintReset_Click(object sender, RoutedEventArgs e)
    {
        _mainWindowTint.SelectedColor = BlurWindow.ParseMainWindowTint(
            Settings.DefaultMainWindowTint
        );
    }
}
