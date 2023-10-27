using SharpVectors.Net;
using Tql.Abstractions;
using Tql.Plugins.Outlook.Services;

namespace Tql.Plugins.Outlook.Data;

internal class OutlookCacheManager : ICacheManager<OutlookData>
{
    public int Version => 1;

    public event EventHandler<CacheExpiredEventArgs>? CacheExpired;

    public Task<OutlookData> Create()
    {
        using var client = new OutlookClient();

        var localPeople = client.FindInContactsFolder();
        var globalPeople = client.FindInGlobalAddressList();

        var unique = localPeople
            .Concat(globalPeople)
            .Distinct(PersonEqualityComparer.Instance)
            .ToImmutableArray();

        return Task.FromResult(new OutlookData(unique));
    }
}
