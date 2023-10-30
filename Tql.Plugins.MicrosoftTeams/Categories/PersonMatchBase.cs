using Tql.Abstractions;
using Tql.Plugins.MicrosoftTeams.Support;
using Tql.Utilities;

namespace Tql.Plugins.MicrosoftTeams.Categories;

internal abstract class PersonMatchBase : ISearchableMatch, ISerializableMatch
{
    private const int MaxResults = 100;

    private readonly IPeopleDirectory _peopleDirectory;

    public abstract string Text { get; }
    public abstract ImageSource Icon { get; }
    public abstract MatchTypeId TypeId { get; }

    protected PersonMatchBase(IPeopleDirectory peopleDirectory)
    {
        _peopleDirectory = peopleDirectory;
    }

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
            return await Task.Run(
                () => context.Filter(GetDtos(allPeople).Select(CreateMatch)).Take(MaxResults),
                cancellationToken
            );
        }

        var people = await _peopleDirectory.Find(text, cancellationToken);

        return GetDtos(people)
            .OrderBy(p => p.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .Take(MaxResults)
            .Select(CreateMatch);
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

    protected abstract IMatch CreateMatch(PersonDto dto);

    public string Serialize()
    {
        return JsonSerializer.Serialize(new RootItemDto(_peopleDirectory.Id));
    }
}

internal record PersonDto(string DirectoryId, string DisplayName, string EmailAddress);
