﻿using Launcher.Abstractions;
using Launcher.Plugins.AzureDevOps.Data;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class BacklogsMatch : CachedMatch<AzureData>, ISerializableMatch
{
    private readonly string _url;

    public override string Text { get; }
    public override ImageSource Icon => Images.Boards;
    public override MatchTypeId TypeId => TypeIds.Backlogs;

    public BacklogsMatch(string text, string url, ICache<AzureData> cache)
        : base(cache)
    {
        _url = url;

        Text = text;
    }

    protected override IEnumerable<IMatch> Create(AzureData data)
    {
        return from project in data.GetConnection(_url).Projects
            from team in project.Teams
            from backlog in project.Backlogs
            select new BacklogMatch(
                new BacklogMatchDto(_url, project.Name, team.Name, backlog.Name)
            );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(new RootItemDto(_url));
    }
}
