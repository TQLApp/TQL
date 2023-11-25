using Tql.Abstractions;

namespace Tql.App.ConfigurationUI;

internal partial class PackageSourceEditWindow
{
    public PackageSourceEditWindow()
    {
        InitializeComponent();
    }

    private void _acceptButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}
