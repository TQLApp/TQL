using Tql.Abstractions;
using Tql.Plugins.MicrosoftTeams.Support;

namespace Tql.Plugins.MicrosoftTeams.Categories;

internal abstract class PersonMatchBase : ISearchableMatch, ISerializableMatch
{
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

        var people = await _peopleDirectory.Find(text, cancellationToken);

        var duplicateDisplayNames = people
            .GroupBy(p => p.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .Where(p => p.Count() > 1)
            .Select(p => p.Key)
            .ToHashSet(StringComparer.CurrentCultureIgnoreCase);

        return people
            .Select(p =>
            {
                var displayName = p.DisplayName;
                if (duplicateDisplayNames.Contains(displayName))
                    displayName += $" - {p.EmailAddress}";

                return CreateMatch(new PersonDto(_peopleDirectory.Id, displayName, p.EmailAddress));
            })
            .OrderBy(p => p.Text, StringComparer.CurrentCultureIgnoreCase);
    }

    protected abstract IMatch CreateMatch(PersonDto dto);

    public string Serialize()
    {
        return JsonSerializer.Serialize(new RootItemDto(_peopleDirectory.Id));
    }
}

internal record PersonDto(string DirectoryId, string DisplayName, string EmailAddress);
