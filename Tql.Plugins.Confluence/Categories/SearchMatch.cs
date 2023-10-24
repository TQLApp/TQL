using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;

namespace Tql.Plugins.Confluence.Categories;

internal class SearchMatch : IRunnableMatch, ISerializableMatch, ICopyableMatch
{
    private readonly SearchMatchDto _dto;

    public string Text =>
        _dto.ContainerTitle == null ? _dto.Title : $"{_dto.ContainerTitle}/{_dto.Title}";
    public ImageSource Icon => Images.Confluence;
    public MatchTypeId TypeId => TypeIds.Search;

    public SearchMatch(SearchMatchDto dto)
    {
        _dto = dto;
    }

    public Task Run(IServiceProvider serviceProvider, Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl(_dto.GetUrl());

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<IClipboard>().CopyUri(Text, _dto.GetUrl());

        return Task.CompletedTask;
    }
}

internal record SearchMatchDto(string Url, string? ContainerTitle, string Title, string ViewUrl)
{
    public string GetUrl()
    {
        var url = ViewUrl;
        if (url.IndexOf("://", StringComparison.Ordinal) == -1)
            url = $"{Url.TrimEnd('/')}/{ViewUrl.TrimStart('/')}";
        return url;
    }
}
