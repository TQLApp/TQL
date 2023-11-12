using System.Text.Json.Serialization;
using Tql.App.Services;

namespace Tql.App.QuickStart;

internal record QuickStartDto(
    QuickStartStep Step,
    QuickStartTool? SelectedTool,
    ImmutableArray<QuickStartTool> CompletedTools
)
{
    private static readonly JsonSerializerOptions JsonSerializerOptions =
        new() { Converters = { new JsonStringEnumConverter() } };

    public static readonly QuickStartDto Empty =
        new(QuickStartStep.Welcome, null, ImmutableArray<QuickStartTool>.Empty);

    public static QuickStartDto FromSettings(Settings settings)
    {
        var quickStart = settings.QuickStart;
        if (quickStart == null)
            return Empty;

        return JsonSerializer.Deserialize<QuickStartDto>(quickStart, JsonSerializerOptions)!;
    }

    public string ToJson() => JsonSerializer.Serialize(this, JsonSerializerOptions);
}

internal enum QuickStartTool
{
    JIRA,
    AzureDevOps,
    GitHub
}

internal enum QuickStartStep
{
    Welcome,
    SelectTool,
    InstallPlugin,
    ConfigurePlugin,
    ListAllCategories,
    SearchInsideCategory,
    ActivateFavorite,
    Completed,
    Dismissed
}
