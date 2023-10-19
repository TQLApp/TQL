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
    }

    public SaveStatus Save()
    {
        if (int.TryParse(_historyInRootResults.Text, out int historyInRootResults))
            _settings.HistoryInRootResults = historyInRootResults;

        _showOnScreenManager.SetScreenIndex(_showOnScreen.SelectedIndex);

        _settings.ShowOnScreen = _showOnScreenManager.ToString();

        return SaveStatus.Success;
    }
}
