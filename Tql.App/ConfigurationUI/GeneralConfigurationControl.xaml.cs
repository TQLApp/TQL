using System.Globalization;
using System.Windows.Forms;
using Tql.Abstractions;
using Tql.App.Services;
using Tql.App.Support;

namespace Tql.App.ConfigurationUI;

internal partial class GeneralConfigurationControl : IConfigurationPage
{
    private readonly Settings _settings;
    private readonly IUI _ui;
    private readonly HotKeyService _hotKeyService;
    private readonly ShowOnScreenManager _showOnScreenManager;
    private readonly bool? _loadedEnableMetricsTelemetry;
    private readonly bool? _loadedEnableExceptionTelemetry;
    private bool _requireRestart;

    public Guid PageId => Guid.Parse("df92b623-a629-465a-bddf-8f36ef6d4fdd");
    public string Title => "General";
    public ConfigurationPageMode PageMode => ConfigurationPageMode.Scroll;

    public GeneralConfigurationControl(Settings settings, IUI ui, HotKeyService hotKeyService)
    {
        _settings = settings;
        _ui = ui;
        _hotKeyService = hotKeyService;

        InitializeComponent();

        InitializeHotKey();
        ConfigureResetButton(_resetHotKey, () => SetHotKey(HotKey.Default));

        _showOnScreenManager = ShowOnScreenManager.Create(settings.ShowOnScreen);

        _showOnScreen.ItemsSource = Enumerable.Range(1, Screen.AllScreens.Length).ToList();
        _showOnScreen.SelectedIndex = _showOnScreenManager.GetScreenIndex();
        ConfigureResetButton(
            _resetShowOnScreen,
            () => _showOnScreen.SelectedIndex = _showOnScreenManager.GetPrimaryScreenIndex()
        );

        _historyInRootResults.ConfigureAsNumericOnly(NumberStyles.None, false);
        _historyInRootResults.Text = settings.HistoryInRootResults?.ToString();
        ConfigureResetButton(_resetHistoryInRootResults, () => _historyInRootResults.Text = null);

        _cacheUpdateInterval.ConfigureAsNumericOnly(NumberStyles.None, false);
        _cacheUpdateInterval.Text = settings.CacheUpdateInterval?.ToString();
        ConfigureResetButton(_resetCacheUpdateInterval, () => _cacheUpdateInterval.Text = null);

        _mainFontSize.ConfigureAsNumericOnly(NumberStyles.None, false);
        _mainFontSize.Text = settings.MainFontSize?.ToString();
        ConfigureResetButton(_resetMainFontSize, () => _mainFontSize.Text = null);

        _mainWindowTint.SelectedColor = BlurWindow.ParseMainWindowTint(
            settings.MainWindowTint ?? Settings.DefaultMainWindowTint
        );
        ConfigureResetButton(
            _resetMainWindowTint,
            () =>
                _mainWindowTint.SelectedColor = BlurWindow.ParseMainWindowTint(
                    Settings.DefaultMainWindowTint
                )
        );

        _theme.ItemsSource = Enum.GetValues(typeof(Theme));
        _theme.SelectedValue = ThemeManager.ParseTheme(settings.Theme);
        ConfigureResetButton(
            _resetTheme,
            () => _theme.SelectedValue = ThemeManager.ParseTheme(null)
        );

        _outerGlow.Value = _settings.TextOuterGlowSize ?? Settings.DefaultTextOuterGlowSize;
        ConfigureResetButton(
            _resetOuterGlow,
            () => _outerGlow.Value = Settings.DefaultTextOuterGlowSize
        );

        _loadedEnableMetricsTelemetry =
            settings.EnableMetricsTelemetry ?? Settings.DefaultEnableMetricsTelemetry;
        _trackMetrics.IsChecked = _loadedEnableMetricsTelemetry;
        ConfigureResetButton(
            _resetTrackMetrics,
            () => _trackMetrics.IsChecked = Settings.DefaultEnableMetricsTelemetry
        );

        _loadedEnableExceptionTelemetry =
            settings.EnableExceptionTelemetry ?? Settings.DefaultEnableExceptionTelemetry;
        _trackErrors.IsChecked = _loadedEnableExceptionTelemetry;
        ConfigureResetButton(
            _resetTrackErrors,
            () => _trackErrors.IsChecked = Settings.DefaultEnableExceptionTelemetry
        );
    }

    public void Initialize(IConfigurationPageContext context)
    {
        context.Closed += (_, _) =>
        {
            if (_requireRestart)
            {
                _ui.ShowConfirmation(
                    this,
                    "Restart required",
                    "You've changed settings that require a restart of the application.",
                    DialogCommonButtons.OK,
                    DialogIcon.Information
                );

                ((UI)_ui).Shutdown(RestartMode.Restart);
            }
        };
    }

    private void ConfigureResetButton(Image image, Action action)
    {
        image.Height = 12;
        image.Width = 12;
        image.Source = Images.Undo;

        image.AttachOnClickHandler(action);
    }

    private void InitializeHotKey()
    {
        foreach (var (key, label) in HotKey.AvailableKeys)
        {
            _hotKeyKey.Items.Add(new ComboBoxItem { Content = label, Tag = key });
        }

        SetHotKey(HotKey.FromSettings(_settings));
    }

    private void SetHotKey(HotKey hotKey)
    {
        _hotKeyWindows.IsChecked = hotKey.Win;
        _hotKeyControl.IsChecked = hotKey.Control;
        _hotKeyAlt.IsChecked = hotKey.Alt;
        _hotKeyShift.IsChecked = hotKey.Shift;

        _hotKeyKey.Items.Cast<ComboBoxItem>().Single(p => (Keys)p.Tag == hotKey.Key).IsSelected =
            true;
    }

    private HotKey GetHotKey()
    {
        return new HotKey(
            _hotKeyWindows.IsChecked.GetValueOrDefault(),
            _hotKeyControl.IsChecked.GetValueOrDefault(),
            _hotKeyAlt.IsChecked.GetValueOrDefault(),
            _hotKeyShift.IsChecked.GetValueOrDefault(),
            (Keys)((ComboBoxItem)_hotKeyKey.SelectedItem).Tag
        );
    }

    public Task<SaveStatus> Save()
    {
        try
        {
            TrySetHotKey();
        }
        catch
        {
            _ui.ShowConfirmation(
                this,
                "Unable to set hot key",
                "The hot key could not be configured. The combination might be "
                    + "invalid or it may already be in use. Please use a different key combination and try again.",
                DialogCommonButtons.OK,
                DialogIcon.Error
            );
            return Task.FromResult(SaveStatus.Failure);
        }

        if (_historyInRootResults.Text.IsEmpty())
            _settings.HistoryInRootResults = null;
        else if (int.TryParse(_historyInRootResults.Text, out int historyInRootResults))
            _settings.HistoryInRootResults = historyInRootResults;

        if (_cacheUpdateInterval.Text.IsEmpty())
            _settings.CacheUpdateInterval = null;
        else if (int.TryParse(_cacheUpdateInterval.Text, out int cacheUpdateInterval))
            _settings.CacheUpdateInterval = cacheUpdateInterval;

        if (_mainFontSize.Text.IsEmpty())
            _settings.MainFontSize = null;
        else if (int.TryParse(_mainFontSize.Text, out int mainFontSize))
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
        var newTheme = theme == Theme.System ? null : theme.ToString();
        if (_settings.Theme != newTheme)
        {
            _settings.Theme = newTheme;
            _requireRestart = true;
        }

        _settings.TextOuterGlowSize = (int)_outerGlow.Value;

        if (_trackMetrics.IsChecked != _loadedEnableMetricsTelemetry)
            _settings.EnableMetricsTelemetry = _trackMetrics.IsChecked;
        if (_trackErrors.IsChecked != _loadedEnableExceptionTelemetry)
            _settings.EnableExceptionTelemetry = _trackErrors.IsChecked;

        return Task.FromResult(SaveStatus.Success);
    }

    private void TrySetHotKey()
    {
        var loadedHotKey = HotKey.FromSettings(_settings);
        var hotKey = GetHotKey();

        if (loadedHotKey != hotKey)
        {
            // This will fail if the hot key is in use elsewhere.
            _hotKeyService.RegisterHotKey(hotKey);

            _settings.HotKey = hotKey == HotKey.Default ? null : hotKey.ToJson();
        }
    }

    private void _mainWindowTintReset_Click(object sender, RoutedEventArgs e)
    {
        _mainWindowTint.SelectedColor = BlurWindow.ParseMainWindowTint(
            Settings.DefaultMainWindowTint
        );
    }
}
