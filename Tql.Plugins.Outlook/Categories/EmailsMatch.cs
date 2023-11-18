using System.Diagnostics;
using Tql.Abstractions;
using Tql.Plugins.Outlook.Services;
using Tql.Utilities;

namespace Tql.Plugins.Outlook.Categories;

internal class EmailsMatch : ISearchableMatch, ISerializableMatch
{
    private const int MaxResults = 100;

    private readonly OutlookPeopleDirectory _outlookPeopleDirectory;
    private readonly IMatchFactory<EmailMatch, PersonDto> _factory;

    public string Text => Labels.EmailsMatch_Label;
    public ImageSource Icon => Images.Outlook;
    public MatchTypeId TypeId => TypeIds.Emails;
    public string SearchHint => Labels.EmailsMatch_SearchHint;

    public EmailsMatch(
        OutlookPeopleDirectory outlookPeopleDirectory,
        IMatchFactory<EmailMatch, PersonDto> factory
    )
    {
        _outlookPeopleDirectory = outlookPeopleDirectory;
        _factory = factory;
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

        var people = await _outlookPeopleDirectory.Find("", cancellationToken);

        Debug.Assert(
            people.Length > 0,
            "The Outlook people directory should return all results on an empty search string"
        );

        return await context.Filter(GetDtos(people).Select(_factory.Create), MaxResults);
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
