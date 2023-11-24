using Tql.Abstractions;

namespace Tql.Plugins.AzureDevOps.ConfigurationUI;

internal partial class EditWindow
{
    private readonly IUI _ui;

    public EditWindow(IUI ui)
    {
        _ui = ui;

        InitializeComponent();
    }

    private void _acceptButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void _documentation_Click(object sender, RoutedEventArgs e)
    {
        _ui.OpenUrl("https://github.com/TQLApp/TQL/wiki/Azure-DevOps-plugin");
    }
}
