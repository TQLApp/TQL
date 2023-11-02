using Tql.Abstractions;
using Tql.Plugins.Outlook.Data;
using Tql.Utilities;

namespace Tql.Plugins.Outlook.Services;

internal class OutlookPeopleDirectory : IPeopleDirectory
{
    private readonly ICache<OutlookData> _cache;

    public string Id => Encryption.Sha1Hash(OutlookPlugin.Id.ToString());
    public string Name => Labels.OutlookPeopleDirectory_Label;

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
            .Where(p => p.DisplayName.Contains(search, StringComparison.CurrentCultureIgnoreCase))
            .ToImmutableArray<IPerson>();
    }
}
