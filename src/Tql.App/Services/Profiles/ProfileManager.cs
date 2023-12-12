using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Interop;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Tql.Abstractions;
using Tql.App.Interop;
using Tql.App.Services.Telemetry;
using Tql.App.Support;
using Path = System.IO.Path;

namespace Tql.App.Services.Profiles;

internal class ProfileManager : IProfileManager
{
    private static readonly uint TaskbarButtonCreatedMessage = NativeMethods.RegisterWindowMessage(
        "TaskbarButtonCreated"
    );

    private static volatile CurrentProfileConfiguration? _currentProfile;

    public static IProfileConfiguration GetCurrentProfile()
    {
        // Suppressing the warning because this should never be an
        // issue. The current profile will only be null on initial
        // load, and there's no risk loading the profile twice.

        // ReSharper disable once NonAtomicCompoundOperator
        _currentProfile ??= LoadCurrentProfile();

        return _currentProfile;
    }

    private static CurrentProfileConfiguration LoadCurrentProfile()
    {
        using var key = CreateKey();

        var configuration = key.GetValue(App.Options.Environment) is string json
            ? JsonSerializer.Deserialize<ProfileConfiguration>(json)!
            : new ProfileConfiguration(
                App.Options.Environment,
                App.Options.Environment,
                Images.DefaultUniverseIcon
            );

        DrawingImage image;

        try
        {
            image = Images.GetImage(configuration.IconName);
        }
        catch
        {
            image = Images.GetImage(Images.DefaultUniverseIcon);
        }

        return new CurrentProfileConfiguration(
            configuration.Name,
            configuration.Title ?? Labels.ProfileManager_DefaultProfile,
            image,
            configuration.IconName
        );
    }

    private static RegistryKey CreateKey()
    {
        return Registry
            .CurrentUser
            .CreateSubKey($@"{Store.RegistryRoot}\{Store.GetEnvironmentName(null)}\Profiles");
    }

    private readonly object _syncRoot = new();
    private readonly ILogger<ProfileManager> _logger;
    private readonly TelemetryService _telemetryService;

    public IProfileConfiguration CurrentProfile => GetCurrentProfile();

    public event EventHandler? CurrentProfileChanged;

    public ProfileManager(ILogger<ProfileManager> logger, TelemetryService telemetryService)
    {
        _logger = logger;
        _telemetryService = telemetryService;

        EventManager.RegisterClassHandler(
            typeof(Window),
            FrameworkElement.LoadedEvent,
            new RoutedEventHandler(OnWindowLoaded)
        );
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        var window = (Window)sender;

        window.Icon = CurrentProfile.Icon;

        var interop = new WindowInteropHelper(window);
        var hwndSource = HwndSource.FromHwnd(interop.Handle);

        var synchronizer = new WindowSynchronizer(window, this);

        synchronizer.InitializeWindowProperties(interop.Handle);

        CurrentProfileChanged += synchronizer.CurrentProfileChanged;

        window.Unloaded += (_, _) =>
        {
            CurrentProfileChanged -= synchronizer.CurrentProfileChanged;
        };

        hwndSource!.AddHook(synchronizer.Hook);
    }

    private void UpdateCurrentProfile()
    {
        _currentProfile = LoadCurrentProfile();

        TrackEvent("ProfileLoaded", _currentProfile.Name, _currentProfile.IconName);

        OnCurrentProfileChanged();
    }

    private void TrackEvent(string name, string? profileName, string iconName)
    {
        using var @event = _telemetryService.CreateEvent(name);

        @event.AddProperty(
            "ProfileNameHash",
            profileName == null
                ? "Default"
                : Utilities.Encryption.Sha1Hash(profileName).Substring(0, 7) // Git short hash length
        );

        @event.AddProperty("IconName", iconName);
    }

    public ImmutableArray<ProfileConfiguration> GetProfiles()
    {
        lock (_syncRoot)
        {
            using var key = CreateKey();

            var profiles = ImmutableArray.CreateBuilder<ProfileConfiguration>();

            if (key.GetValue(null) == null)
            {
                key.SetValue(
                    null,
                    JsonSerializer.Serialize(
                        new ProfileConfiguration(
                            null,
                            Labels.ProfileManager_DefaultProfile,
                            Images.DefaultUniverseIcon
                        )
                    )
                );
            }

            if (App.Options.Environment != null && key.GetValue(App.Options.Environment) == null)
            {
                key.SetValue(
                    App.Options.Environment,
                    JsonSerializer.Serialize(
                        new ProfileConfiguration(
                            App.Options.Environment,
                            App.Options.Environment,
                            Images.DefaultUniverseIcon
                        )
                    )
                );
            }

            foreach (
                var name in key.GetValueNames()
                    .OrderBy(p => p, StringComparer.CurrentCultureIgnoreCase)
            )
            {
                if (key.GetValue(name) is string json)
                    profiles.Add(JsonSerializer.Deserialize<ProfileConfiguration>(json)!);
            }

            return profiles.ToImmutable();
        }
    }

    public void AddProfile(ProfileConfiguration profile)
    {
        if (profile.Name != null && Regex.IsMatch(profile.Name, @"[^\w]"))
            throw new InvalidOperationException("Profile name contains invalid characters");

        lock (_syncRoot)
        {
            var profiles = GetProfiles();

            if (
                profiles.Any(
                    p => string.Equals(p.Name, profile.Name, StringComparison.OrdinalIgnoreCase)
                )
            )
                throw new InvalidOperationException("Profile with name already exists");

            using var key = CreateKey();

            key.SetValue(profile.Name, JsonSerializer.Serialize(profile));
        }

        CreateIcons(profile, GetTargetFileName());

        if (IsCurrentProfile(profile.Name))
            UpdateCurrentProfile();

        TrackEvent("ProfileAdded", profile.Name, profile.IconName);
    }

    public void UpdateProfile(ProfileConfiguration profile)
    {
        ProfileConfiguration? oldProfile;

        lock (_syncRoot)
        {
            oldProfile = GetProfiles()
                .SingleOrDefault(
                    p => string.Equals(p.Name, profile.Name, StringComparison.OrdinalIgnoreCase)
                );

            if (oldProfile == null)
                throw new InvalidOperationException("Profile does not exist");

            using var key = CreateKey();

            key.SetValue(profile.Name, JsonSerializer.Serialize(profile));
        }

        var targetFileName = GetTargetFileName();

        DeleteIcons(oldProfile);
        CreateIcons(profile, targetFileName);

        if (IsCurrentProfile(profile.Name))
            UpdateCurrentProfile();

        TrackEvent("ProfileUpdated", profile.Name, profile.IconName);
    }

    public void DeleteProfile(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new InvalidOperationException("The default profile cannot be deleted");
        if (IsCurrentProfile(name))
            throw new InvalidOperationException("The current profile cannot be deleted");

        var profile = GetProfiles()
            .SingleOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
        if (profile == null)
            throw new InvalidOperationException("Profile with name does not exists");

        // Rename the data folder. This is the make sure the app isn't running.
        var dataFolder = Store.GetDataFolder(profile.Name);
        var renamedDataFolder = GetRenamedDataFolder(dataFolder);

        try
        {
            Directory.Move(dataFolder, renamedDataFolder);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "The profile data could not be deleted. The app with this profile may still be running. Close it and try again.",
                ex
            );
        }

        lock (_syncRoot)
        {
            using var key = CreateKey();

            key.DeleteValue(name);
        }

        DeleteIcons(profile);
        DeleteFolder(renamedDataFolder);
        DeleteFolder(Store.GetLocalDataFolder(profile.Name));
        DeleteRegistryKey(Store.GetEnvironmentName(profile.Name));

        TrackEvent("ProfileDeleted", profile.Name, profile.IconName);
    }

    private static string GetRenamedDataFolder(string dataFolder)
    {
        var baseFolderName = $"{dataFolder}-DELETE";

        for (var i = 0; ; i++)
        {
            var folderName = baseFolderName;
            if (i > 0)
                folderName += i;

            if (!Directory.Exists(folderName))
                return folderName;
        }
    }

    private static bool IsCurrentProfile(string? name) =>
        string.Equals(App.Options.Environment, name, StringComparison.OrdinalIgnoreCase);

    private void DeleteFolder(string path)
    {
        try
        {
            Directory.Delete(path, true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not delete folder '{Folder}'", path);
        }
    }

    private void DeleteRegistryKey(string name)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(Store.RegistryRoot);

            key.DeleteSubKeyTree(name, false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not registry key '{Name}'", name);
        }
    }

    private void CreateIcons(ProfileConfiguration profile, string targetFileName)
    {
        var profileFolder = Store.GetDataFolder(profile.Name);
        Directory.CreateDirectory(profileFolder);

        var iconFileName = Path.Combine(profileFolder, "icon.ico");
        CreateIconFile(iconFileName, profile.IconName);

        var fileName = GetShellLinkFileName(profile);

        CreateShellLink(GetProgramsFolder(), false);
        CreateShellLink(GetStartupFolder(), true);

        ShellLink.NotifyShellLinksChanged();

        void CreateShellLink(string folder, bool isSilent)
        {
            var arguments = "";
            if (profile.Name != null)
                arguments += $"--env \"{profile.Name}\"";
            if (isSilent)
                arguments += " --silent";

            var shellLink = new ShellLink
            {
                IconPath = iconFileName,
                IconIndex = 0,
                Target = targetFileName,
                WorkingDirectory = Path.GetDirectoryName(targetFileName),
                Description = Labels.ApplicationShellLinkDescription,
                Arguments = arguments,
            };

            shellLink.Save(Path.Combine(folder, fileName));
        }
    }

    private string GetTargetFileName()
    {
        var fileName = Path.Combine(GetProgramsFolder(), GetShellLinkFileName(null));

        var shellLink = new ShellLink(fileName);

        return shellLink.Target;
    }

    private void CreateIconFile(string fileName, string iconName)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(fileName)!);

        var image = Images.GetImage(iconName);

        using var source = IconBuilder.Build(image);
        using var target = File.Create(fileName);

        source.CopyTo(target);
    }

    private void DeleteIcons(ProfileConfiguration profile)
    {
        var fileName = GetShellLinkFileName(profile);

        Delete(Path.Combine(GetProgramsFolder(), fileName));
        Delete(Path.Combine(GetStartupFolder(), fileName));

        ShellLink.NotifyShellLinksChanged();

        void Delete(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cannot delete file '{Path}'", path);
            }
        }
    }

    private string GetStartupFolder()
    {
        return Environment.GetFolderPath(Environment.SpecialFolder.Startup);
    }

    private string GetProgramsFolder()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Programs),
            Labels.ApplicationTitle
        );
    }

    private string GetShellLinkFileName(ProfileConfiguration? profile)
    {
        var fileName = Labels.ApplicationTitle;
        if (profile?.Name != null)
            fileName += $" - {profile.Title}";

        return $"{fileName}.lnk";
    }

    public string GetNextProfileName()
    {
        var profileNames = GetProfiles()
            .Where(p => p.Name != null)
            .Select(p => p.Name!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        for (var i = 1; ; i++)
        {
            var profileName = $"Profile{i}";
            if (!profileNames.Contains(profileName))
                return profileName;
        }
    }

    private class CurrentProfileConfiguration : IProfileConfiguration
    {
        // We need to track this icon to make sure it's not disposed.
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Icon _icon;

        public string? Name { get; }
        public string Title { get; }
        public ImageSource Image { get; }
        public ImageSource Icon { get; }
        public string IconName { get; }

        public CurrentProfileConfiguration(
            string? name,
            string title,
            ImageSource image,
            string iconName
        )
        {
            Name = name;
            Title = title;
            Image = image;
            IconName = iconName;

            using (var iconStream = IconBuilder.Build(image))
            {
                _icon = new Icon(iconStream);
            }

            Icon = Imaging.CreateBitmapSourceFromHIcon(
                _icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions()
            );
        }
    }

    protected virtual void OnCurrentProfileChanged() =>
        CurrentProfileChanged?.Invoke(this, EventArgs.Empty);

    private class WindowSynchronizer(Window window, ProfileManager owner)
    {
        public void CurrentProfileChanged(object? sender, EventArgs e)
        {
            window.Icon = owner.CurrentProfile.Icon;
        }

        public IntPtr Hook(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
        {
            if (msg == TaskbarButtonCreatedMessage)
                InitializeWindowProperties(hwnd);

            return IntPtr.Zero;
        }

        public void InitializeWindowProperties(IntPtr hwnd)
        {
            using var propertyStore = new WindowPropertyStore(hwnd);

            propertyStore.SetValue(PropertyStoreProperty.AppUserModel_PreventPinning, true);
        }
    }
}
