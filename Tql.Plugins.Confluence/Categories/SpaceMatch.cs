using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.Confluence.Services;

namespace Tql.Plugins.Confluence.Categories;

internal class SpaceMatch : IRunnableMatch, ISerializableMatch, ICopyableMatch
{
    private readonly SpaceMatchDto _dto;

    public string Text => $"{_dto.Name} Space";
    public ImageSource Icon { get; }
    public MatchTypeId TypeId => TypeIds.Space;

    public SpaceMatch(SpaceMatchDto dto, IconCacheManager iconCacheManager)
    {
        _dto = dto;

        Icon = iconCacheManager.GetIcon(dto.Icon) ?? Images.Confluence;
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

internal record SpaceMatchDto(string Url, string Key, string Name, string ViewUrl, string Icon)
{
    public string GetUrl()
    {
        var url = ViewUrl;
        if (url.IndexOf("://", StringComparison.Ordinal) == -1)
            url = $"{Url.TrimEnd('/')}/{ViewUrl.TrimStart('/')}";
        return url;
    }
};
