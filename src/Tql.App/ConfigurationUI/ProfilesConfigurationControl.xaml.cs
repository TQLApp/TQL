using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.App.Services.Profiles;
using Tql.Utilities;
using ButtonBase = System.Windows.Controls.Primitives.ButtonBase;

namespace Tql.App.ConfigurationUI;

internal partial class ProfilesConfigurationControl : IConfigurationPage
{
    private readonly ProfileManager _profileManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly IUI _ui;

    private new ProfileConfigurationDto DataContext => (ProfileConfigurationDto)base.DataContext;

    public Guid PageId => Constants.PackageManagerPageId;
    public string Title => Labels.ProfilesConfiguration_Profiles;
    public ConfigurationPageMode PageMode => ConfigurationPageMode.AutoSize;

    public ProfilesConfigurationControl(
        IProfileManager profileManager,
        IServiceProvider serviceProvider,
        IUI ui
    )
    {
        _profileManager = (ProfileManager)profileManager;
        _serviceProvider = serviceProvider;
        _ui = ui;

        InitializeComponent();

        base.DataContext = ProfileConfigurationDto.FromConfiguration(_profileManager);
    }

    public void Initialize(IConfigurationPageContext context) { }

    public Task<SaveStatus> Save()
    {
        return Task.FromResult(SaveStatus.Success);
    }

    private void _add_Click(object? sender, RoutedEventArgs e) => Edit(null);

    private void _edit_Click(object sender, RoutedEventArgs e) =>
        Edit((ProfileDto)_sources.SelectedItem);

    private void Edit(ProfileDto? profileDto)
    {
        var editProfile =
            profileDto?.Clone()
            ?? new ProfileDto
            {
                Name = _profileManager.GetNextProfileName(),
                IconName = Images.DefaultUniverseIcon
            };

        var window = _serviceProvider.GetRequiredService<ProfileEditWindow>();
        window.Owner = Window.GetWindow(this);
        window.DataContext = editProfile;

        if (!window.ShowDialog().GetValueOrDefault())
            return;

        if (profileDto != null)
        {
            _profileManager.UpdateProfile(editProfile.ToConfiguration());

            DataContext.Profiles[_sources.SelectedIndex] = editProfile;
        }
        else
        {
            _profileManager.AddProfile(editProfile.ToConfiguration());

            DataContext.Profiles.Add(editProfile);

            _ui.ShowAlert(
                this,
                Labels.ProfilesConfiguration_CreatedNewProfile,
                Labels.ProfilesConfiguration_CreatedNewProfileSubtitle,
                icon: DialogIcon.Information
            );
        }
    }

    private void _delete_Click(object? sender, RoutedEventArgs e)
    {
        var profileDto = (ProfileDto)_sources.SelectedItem;

        if (string.IsNullOrEmpty(profileDto.Name))
        {
            _ui.ShowAlert(this, Labels.ProfilesConfiguration_CannotDeleteDefaultProfile);
            return;
        }

        if (
            string.Equals(
                profileDto.Name,
                App.Options.Environment,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            _ui.ShowAlert(
                this,
                Labels.ProfilesConfiguration_CannotDeleteCurrentProfile,
                Labels.ProfilesConfiguration_CannotDeleteCurrentProfileSubtitle
            );
            return;
        }

        var result = _ui.ShowConfirmation(
            this,
            Labels.ProfilesConfiguration_AreYouSureDeleteProfile,
            Labels.ProfilesConfiguration_AreYouSureDeleteProfileSubtitle
        );
        if (result == DialogResult.Yes)
        {
            _profileManager.DeleteProfile(profileDto.Name);

            DataContext.Profiles.Remove(profileDto);
        }
    }

    private void _sources_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left && _edit.IsEnabled)
            _edit.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
    }
}
