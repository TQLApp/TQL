namespace Tql.App.ConfigurationUI;

internal partial class ProfileEditWindow
{
    public ProfileEditWindow(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();

        _icons.ItemsSource = Images.UniverseImages;
    }

    private void _acceptButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}
