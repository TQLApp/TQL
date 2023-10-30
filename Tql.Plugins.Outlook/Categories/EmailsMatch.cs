using System.Diagnostics;
using Tql.Abstractions;
using Tql.Plugins.Outlook.Support;
using Tql.Utilities;

namespace Tql.Plugins.Outlook.Categories;

internal class EmailsMatch : ISearchableMatch, ISerializableMatch
{
    private const int MaxResults = 100;

    private readonly IPeopleDirectory _peopleDirectory;

    public string Text => "Email";
    public ImageSource Icon => Images.Outlook;
    public MatchTypeId TypeId => TypeIds.Emails;

    public EmailsMatch(IPeopleDirectory peopleDirectory)
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

        var people = await _peopleDirectory.Find("", cancellationToken);

        Debug.Assert(
            people.Length > 0,
            "The Outlook people directory should return all results on an empty search string"
        );

        return await Task.Run(
            () => context.Filter(GetDtos(people).Select(p => new EmailMatch(p))).Take(MaxResults),
            cancellationToken
        );
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

            return new PersonDto(displayName, p.EmailAddress);
        });
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(new RootItemDto());
    }
}

internal record PersonDto(string DisplayName, string EmailAddress);
