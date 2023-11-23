using Tql.Abstractions;
using Tql.Plugins.MicrosoftTeams.Services;
using Tql.Plugins.MicrosoftTeams.Support;
using Tql.Utilities;

namespace Tql.Plugins.MicrosoftTeams.Categories;

internal abstract class PeopleMatchBase<T>(
    RootItemDto dto,
    string label,
    ConfigurationManager configurationManager,
    IMatchFactory<T, PersonDto> factory
) : ISearchableMatch, ISerializableMatch
    where T : IMatch
{
    private const int MaxResults = 100;

    private readonly IPeopleDirectory _peopleDirectory = configurationManager.GetDirectory(dto.Id)!;

    public string Text => MatchUtils.GetMatchLabel(label, configurationManager, dto.Id);

    public abstract ImageSource Icon { get; }
    public abstract MatchTypeId TypeId { get; }
    public string SearchHint => Labels.PeopleMatchBase_SearchHint;

    public async Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        if (text.IsWhiteSpace())
            return Array.Empty<IMatch>();

        await context.DebounceDelay(cancellationToken);

        // The semantics for the people directory are that if it's willing
        // to answer an empty search, it promises it's fast and it's
        // supposed to have this data cached. We prefer the cached result
        // set because this allows use to leverage the search functionality
        // built into TQL.

        var allPeople = await _peopleDirectory.Find("", cancellationToken);
        if (allPeople.Length > 0)
        {
            return context
                .Filter(GetDtos(allPeople).Select(p => (IMatch)factory.Create(p)))
                .Take(MaxResults);
        }

        var people = await _peopleDirectory.Find(text, cancellationToken);

        return GetDtos(people)
            .OrderBy(p => p.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .Take(MaxResults)
            .Select(p => (IMatch)factory.Create(p));
    }

    private IEnumerable<PersonDto> GetDtos(ImmutableArray<IPerson> people)
    {
        var duplicateDisplayNames = people
            .GroupBy(p => p.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .Where(p => p.Count() > 1)
            .Select(p => p.Key)
            .ToHashSet(StringComparer.CurrentCultureIgnoreCase);

        return people.Select(p =>
        {
            var displayName = p.DisplayName;
            if (duplicateDisplayNames.Contains(displayName))
                displayName += $" - {p.EmailAddress}";

            return new PersonDto(_peopleDirectory.Id, displayName, p.EmailAddress);
        });
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(new RootItemDto(_peopleDirectory.Id));
    }
}

internal record PersonDto(string DirectoryId, string DisplayName, string EmailAddress);
