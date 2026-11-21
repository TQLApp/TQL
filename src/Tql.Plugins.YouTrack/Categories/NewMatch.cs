using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.YouTrack.Services;
using Tql.Plugins.YouTrack.Support;
using Tql.Utilities;

namespace Tql.Plugins.YouTrack.Categories;

internal class NewMatch(NewMatchDto dto, IconCacheManager iconCacheManager)
    : IRunnableMatch,
        ISerializableMatch,
        ICopyableMatch
{
    public string Text => MatchText.Path(dto.ProjectName, Labels.NewMatch_NewIssue);
    public ImageSource Icon { get; } =
        (dto.IconUrl != null ? iconCacheManager.GetIcon(new IconKey(dto.Url, dto.IconUrl)) : null)
        ?? Images.Issues;
    public MatchTypeId TypeId => TypeIds.New;

    public Task Run(IServiceProvider serviceProvider, IWin32Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl(dto.GetUrl());

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<IClipboard>().CopyUri(Text, dto.GetUrl());

        return Task.CompletedTask;
    }
}

internal record NewMatchDto(
    string Url,
    string ProjectShortName,
    string ProjectName,
    string? IconUrl
)
{
    public string GetUrl() => YouTrackUrls.GetNewIssueUrl(Url, ProjectShortName);
};
