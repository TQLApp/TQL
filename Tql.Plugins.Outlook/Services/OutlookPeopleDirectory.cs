using Tql.Abstractions;
using Tql.Plugins.Outlook.Data;
using Tql.Utilities;

namespace Tql.Plugins.Outlook.Services;

internal class OutlookPeopleDirectory(ICache<OutlookData> cache) : IPeopleDirectory
{
    public string Id => Encryption.Sha1Hash(OutlookPlugin.Id.ToString());
    public string Name => Labels.OutlookPeopleDirectory_Label;

    public async Task<ImmutableArray<IPerson>> Find(
        string search,
        CancellationToken cancellationToken = default
    )
    {
        var data = await cache.Get();

        if (search.IsWhiteSpace())
            return data.People.CastArray<IPerson>();

        return data.People
            .Where(p => p.DisplayName.Contains(search, StringComparison.CurrentCultureIgnoreCase))
            .ToImmutableArray<IPerson>();
    }
}
