﻿using Tql.Abstractions;
using Tql.Plugins.Outlook.Data;
using Tql.Plugins.Outlook.Support;

namespace Tql.Plugins.Outlook.Services;

internal class OutlookPeopleDirectory : IPeopleDirectory
{
    private readonly ICache<OutlookData> _cache;

    public string Id => Encryption.Hash(OutlookPlugin.Id.ToString());
    public string Name => "Outlook";

    public OutlookPeopleDirectory(ICache<OutlookData> cache)
    {
        _cache = cache;
    }

    public async Task<ImmutableArray<IPerson>> Find(
        string search,
        CancellationToken cancellationToken = default
    )
    {
        var cache = await _cache.Get();

        if (search.IsWhiteSpace())
            return cache.People.CastArray<IPerson>();

        return cache.People
            .Where(
                p => p.DisplayName.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) != -1
            )
            .ToImmutableArray<IPerson>();
    }
}