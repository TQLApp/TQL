using Launcher.Abstractions;
using Launcher.Plugins.AzureDevOps.Services;

namespace Launcher.Plugins.AzureDevOps.Categories.People;

internal class EmailsMatch : GraphUserMatchBase
{
    public override string Text { get; }
    public override ImageSource Icon => Images.Outlook;
    public override MatchTypeId TypeId => TypeIds.Emails;

    public EmailsMatch(string text, string url, AzureDevOpsApi api)
        : base(url, api)
    {
        Text = text;
    }

    protected override IMatch CreateMatch(GraphUserDto dto)
    {
        return new EmailMatch(dto);
    }
}
