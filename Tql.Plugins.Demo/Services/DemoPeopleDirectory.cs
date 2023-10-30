using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.Demo.Services;

internal class DemoPeopleDirectory : IPeopleDirectory
{
    private static readonly Regex WhitespaceRe = new Regex(@"\w+", RegexOptions.Compiled);

    private readonly ImmutableArray<IPerson> _people;

    public string Id => Encryption.Sha1Hash(DemoPlugin.Id.ToString());
    public string Name => "Demo";

    public DemoPeopleDirectory()
    {
        _people = (
            from name in PersonNames.Generate().Take(45_000)
            select new Person(
                $"{name.Surname}, {name.Name}",
                WhitespaceRe.Replace($"{name.Name}.{name.Surname}@outlook.com", "")
            )
        ).ToImmutableArray<IPerson>();
    }

    public Task<ImmutableArray<IPerson>> Find(
        string search,
        CancellationToken cancellationToken = default
    )
    {
        if (search.IsWhiteSpace())
            return Task.FromResult(_people);

        return Task.FromResult(
            _people
                .Where(
                    p => p.DisplayName.Contains(search, StringComparison.CurrentCultureIgnoreCase)
                )
                .ToImmutableArray()
        );
    }

    private record Person(string DisplayName, string EmailAddress) : IPerson;
}
