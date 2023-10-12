using Launcher.Abstractions;
using Launcher.Plugins.AzureDevOps.Services;

namespace Launcher.Plugins.AzureDevOps.Categories.People;

internal class TeamsChatsMatch : GraphUserMatchBase
{
    public override string Text { get; }
    public override ImageSource Icon => Images.Teams;
    public override MatchTypeId TypeId => TypeIds.TeamsChats;

    public TeamsChatsMatch(string text, string url, AzureDevOpsApi api)
        : base(url, api)
    {
        Text = text;
    }

    protected override IMatch CreateMatch(GraphUserDto dto)
    {
        return new TeamsChatMatch(dto);
    }
}
