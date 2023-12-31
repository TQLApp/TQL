﻿using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.Confluence.Categories;

internal class SearchMatch(SearchMatchDto dto) : IRunnableMatch, ISerializableMatch, ICopyableMatch
{
    public string Text =>
        MatchText.Path(
            new[] { dto.ContainerTitle, dto.Title },
            MatchPathOptions.RemoveEmptyEntries
        );

    public ImageSource Icon => Images.Confluence;
    public MatchTypeId TypeId => TypeIds.Search;

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

internal record SearchMatchDto(string Url, string? ContainerTitle, string Title, string ViewUrl)
{
    public string GetUrl()
    {
        var url = ViewUrl;
        if (!url.Contains("://", StringComparison.Ordinal))
            url = $"{Url.TrimEnd('/')}/{ViewUrl.TrimStart('/')}";
        return url;
    }
}
