using System.Collections.ObjectModel;
using Tql.App.Services.Profiles;
using Tql.App.Support;
using Tql.Utilities;

namespace Tql.App.ConfigurationUI;

internal class ProfileConfigurationDto
{
    public ObservableCollection<ProfileDto> Profiles { get; } = new();

    public static ProfileConfigurationDto FromConfiguration(ProfileManager profileManager)
    {
        var result = new ProfileConfigurationDto();

        result
            .Profiles
            .AddRange(
                profileManager
                    .GetProfiles()
                    .Select(
                        p =>
                            new ProfileDto
                            {
                                Name = p.Name,
                                Title = p.Title,
                                IconName = p.IconName
                            }
                    )
            );

        return result;
    }
}

internal class ProfileDto : DtoBase
{
    public string? Name
    {
        get => (string?)GetValue();
        set => SetValue(value);
    }

    public string? Title
    {
        get => (string?)GetValue();
        set => SetValue(value);
    }

    public string? IconName
    {
        get => (string?)GetValue();
        set => SetValue(value);
    }

    public ProfileDto()
    {
        AddProperty(nameof(Name), null, CoerceEmptyStringToNull);
        AddProperty(nameof(Title), ValidateNotEmpty, CoerceEmptyStringToNull);
        AddProperty(nameof(IconName), ValidateNotEmpty, CoerceEmptyStringToNull);
    }

    public ProfileDto Clone() => (ProfileDto)Clone(new ProfileDto());

    public ProfileConfiguration ToConfiguration()
    {
        return new ProfileConfiguration(Name, Title, IconName!);
    }
}
