using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Tql.Plugins.Outlook.Services;

namespace Tql.Plugins.Outlook.Data;

internal class OutlookCacheManager : ICacheManager<OutlookData>
{
    private readonly ILogger<OutlookClient> _clientLogger;
    public int Version => 1;

    public event EventHandler<CacheExpiredEventArgs>? CacheExpired;

    public OutlookCacheManager(ILogger<OutlookClient> clientLogger)
    {
        _clientLogger = clientLogger;
    }

    public Task<OutlookData> Create()
    {
        using var client = new OutlookClient(_clientLogger);

        var localPeople = client.FindInContactsFolder();
        var globalPeople = client.FindInGlobalAddressList();

        var unique = localPeople
            .Concat(globalPeople)
            .Distinct(PersonEqualityComparer.Instance)
            .ToImmutableArray();

        return Task.FromResult(new OutlookData(unique));
    }

    protected virtual void OnCacheExpired(CacheExpiredEventArgs e) => CacheExpired?.Invoke(this, e);
}
