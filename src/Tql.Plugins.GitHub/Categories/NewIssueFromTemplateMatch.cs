using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class NewIssueFromTemplateMatch(NewIssueFromTemplateMatchDto dto)
    : IRunnableMatch,
        ICopyableMatch,
        ISerializableMatch
{
    public string Text =>
        MatchText.Path($"{dto.Owner}/{dto.Repository}", Labels.NewMatch_NewIssue, dto.TemplateName);
    public ImageSource Icon => Images.Issue;
    public MatchTypeId TypeId => TypeIds.NewIssue;

    public Task Run(IServiceProvider serviceProvider, IWin32Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl(dto.GetUrl());

        return Task.CompletedTask;
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<IClipboard>().CopyUri(Text, dto.GetUrl());

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }
}

internal record NewIssueFromTemplateMatchDto(
    Guid Id,
    string Owner,
    string Repository,
    string TemplateName,
    string TemplateFileName
)
{
    public string GetUrl() =>
        $"https://github.com/{Uri.EscapeDataString(Owner)}/{Uri.EscapeDataString(Repository)}/issues/new?template={Uri.EscapeDataString(TemplateFileName)}";
}
