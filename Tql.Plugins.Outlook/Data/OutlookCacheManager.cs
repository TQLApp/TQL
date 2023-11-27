using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Tql.Plugins.Outlook.Services;
using Tql.Plugins.Outlook.Services.Interop;

namespace Tql.Plugins.Outlook.Data;

internal class OutlookCacheManager : ICacheManager<OutlookData>
{
    private readonly ILogger<OutlookClient> _clientLogger;
    private readonly ConfigurationManager _configurationManager;
    private readonly ILogger<OutlookCacheManager> _logger;

    public OutlookCacheManager(
        ILogger<OutlookClient> clientLogger,
        ConfigurationManager configurationManager,
        ILogger<OutlookCacheManager> logger
    )
    {
        _clientLogger = clientLogger;
        _configurationManager = configurationManager;
        _logger = logger;

        configurationManager.Changed += (_, _) =>
            OnCacheInvalidationRequired(new CacheInvalidationRequiredEventArgs(true));
    }

    public int Version => 1;

    public event EventHandler<CacheInvalidationRequiredEventArgs>? CacheInvalidationRequired;

    public Task<OutlookData> Create()
    {
        using var client = new OutlookClient(_clientLogger);

        var localPeople = client.FindInContactsFolder();
        var globalPeople = client.FindInGlobalAddressList();

        var people = localPeople.Concat(globalPeople);

        var nameFormat = _configurationManager.Configuration.NameFormat;

        if (nameFormat == NameFormat.LastNameCommaFirstName)
            people = ReformatNames(people);

        var unique = people.Distinct(PersonEqualityComparer.Instance).ToImmutableArray();

        return Task.FromResult(new OutlookData(unique));
    }

    private IEnumerable<Person> ReformatNames(IEnumerable<Person> people)
    {
        var total = 0;
        var zeroCommas = 0;
        var multipleCommas = 0;

        foreach (var person in people)
        {
            total++;

            var firstPos = person.DisplayName.IndexOf(',');
            if (firstPos == -1)
            {
                zeroCommas++;

                yield return person;
                continue;
            }

            var lastPos = person.DisplayName.LastIndexOf(',');

            if (firstPos != lastPos)
                multipleCommas++;

            var lastName = person.DisplayName[..lastPos].Trim();
            var firstName = person.DisplayName[(lastPos + 1)..].Trim();

            yield return person with
            {
                DisplayName = firstName + " " + lastName
            };
        }

        _logger.LogInformation(
            "Reformatted {Total} people, {ZeroCommas} had zero commas, {MultipleCommas} had more than one comma",
            total,
            zeroCommas,
            multipleCommas
        );
    }

    protected virtual void OnCacheInvalidationRequired(CacheInvalidationRequiredEventArgs e) =>
        CacheInvalidationRequired?.Invoke(this, e);
}
