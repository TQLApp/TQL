namespace Tql.App.ConfigurationUI;

internal partial class ProfileEditWindow
{
    public ProfileEditWindow()
    {
        InitializeComponent();

        _icons.ItemsSource = Images
            .UniverseImages
            .Select(p => new ProfileEditIcon(p, Images.GetImage(p)))
            .ToList();
    }

    private void BaseWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        var icons = (List<ProfileEditIcon>)_icons.ItemsSource;
        var profileDto = (ProfileDto)DataContext;

        var icon = icons.FirstOrDefault(
            p => string.Equals(p.Name, profileDto.IconName, StringComparison.OrdinalIgnoreCase)
        );

        if (icon == null)
        {
            icon = icons.Single(
                p =>
                    string.Equals(
                        p.Name,
                        Images.DefaultUniverseIcon,
                        StringComparison.OrdinalIgnoreCase
                    )
            );
        }

        _icons.SelectedItem = icon;

        if (string.IsNullOrEmpty(profileDto.Name))
            _title.IsEnabled = false;
    }

    private void _acceptButton_Click(object sender, RoutedEventArgs e)
    {
        var profileDto = (ProfileDto)DataContext;
        profileDto.IconName = ((ProfileEditIcon)_icons.SelectedItem).Name;

        DialogResult = true;
    }
}

internal record ProfileEditIcon(string Name, DrawingImage Icon);
