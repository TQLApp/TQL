using Tql.App.Services;

namespace Tql.App.QuickStart;

internal record QuickStartDto(
    bool IsDismissed = false,
    bool WelcomeShown = false,
    QuickStartTool? SelectedTool = null
)
{
    public static readonly QuickStartDto Empty = new();

    public static QuickStartDto FromSettings(Settings settings)
    {
        var quickStart = settings.QuickStart;
        if (quickStart == null)
            return Empty;

        return JsonSerializer.Deserialize<QuickStartDto>(quickStart)!;
    }

    public string ToJson() => JsonSerializer.Serialize(this);
}

internal enum QuickStartTool
{
    JIRA,
    AzureDevOps,
    GitHub,
    Outlook
}
