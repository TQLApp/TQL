using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Tql.App.Interop;
using Tql.App.Support;
using Path = System.IO.Path;

namespace Tql.App.Services.Profiles;

internal class ProfileManager(ILogger<ProfileManager> logger)
{
    private readonly object _syncRoot = new();

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
    }

    public void DeleteProfile(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new InvalidOperationException("The default profile cannot be deleted");
        if (string.Equals(App.Options.Environment, name, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("The current profile cannot be deleted");

        ProfileConfiguration? profile;

        lock (_syncRoot)
        {
            profile = GetProfiles()
                .SingleOrDefault(
                    p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)
                );
            if (profile == null)
                throw new InvalidOperationException("Profile with name does not exists");

            using var key = CreateKey();

            key.DeleteValue(name);
        }

        DeleteIcons(profile);
        DeleteFolder(Store.GetDataFolder(profile.Name));
        DeleteFolder(Store.GetLocalDataFolder(profile.Name));
        DeleteRegistryKey(Store.GetEnvironmentName(profile.Name));
    }

    private void DeleteFolder(string path)
    {
        try
        {
            Directory.Delete(path, true);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not delete folder '{Folder}'", path);
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
            logger.LogWarning(ex, "Could not registry key '{Name}'", name);
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
        var image = Images.GetImage(iconName);

        var icon = IconBuilder.Build(image);

        using var stream = File.Create(fileName);

        icon.Save(stream);
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
                logger.LogWarning(ex, "Cannot delete file '{Path}'", path);
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

    private RegistryKey CreateKey()
    {
        return Registry
            .CurrentUser
            .CreateSubKey($@"{Store.RegistryRoot}\{Store.GetEnvironmentName(null)}\Profiles");
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
}
