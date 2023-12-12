using Tql.Abstractions;

namespace Tql.Plugins.Jira.ConfigurationUI;

internal partial class ConnectionEditWindow
{
    private readonly IUI _ui;

    public ConnectionEditWindow(IUI ui)
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
        _ui.OpenUrl("https://github.com/TQLApp/TQL/wiki/JIRA-plugin");
    }
}
