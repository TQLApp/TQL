using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Tql.Plugins.Outlook.Services;

namespace Tql.Plugins.Outlook.Data;

internal class OutlookCacheManager(ILogger<OutlookClient> clientLogger) : ICacheManager<OutlookData>
{
    public int Version => 1;

    public event EventHandler<CacheInvalidationRequiredEventArgs>? CacheInvalidationRequired;

    public Task<OutlookData> Create()
    {
        using var client = new OutlookClient(clientLogger);

        var localPeople = client.FindInContactsFolder();
        var globalPeople = client.FindInGlobalAddressList();

        var unique = localPeople
            .Concat(globalPeople)
            .Distinct(PersonEqualityComparer.Instance)
            .ToImmutableArray();

        return Task.FromResult(new OutlookData(unique));
    }

    protected virtual void OnCacheInvalidationRequired(CacheInvalidationRequiredEventArgs e) =>
        CacheInvalidationRequired?.Invoke(this, e);
}
