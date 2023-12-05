using System.Text.RegularExpressions;
using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.Demo.Services;

internal class DemoPeopleDirectory(string name, string locale) : IPeopleDirectory
{
    private static readonly Regex WhitespaceRe = new(@"\s+", RegexOptions.Compiled);

    private ImmutableArray<IPerson>? _people;

    public string Id { get; } = Encryption.Sha1Hash($"{DemoPlugin.Id}|{locale}");
    public string Name { get; } = name;

    public Task<ImmutableArray<IPerson>> Find(
        string search,
        CancellationToken cancellationToken = default
    )
    {
        if (search.IsWhiteSpace())
            return Task.FromResult(GetPeople());

        return Task.FromResult(
            GetPeople()
                .Where(
                    p => p.DisplayName.Contains(search, StringComparison.CurrentCultureIgnoreCase)
                )
                .ToImmutableArray()
        );
    }

    private ImmutableArray<IPerson> GetPeople()
    {
        return _people ??= (
            from personName in PersonNames.Generate(locale).Take(45_000)
            select new Person(
                $"{personName.Surname}, {personName.Name}",
                WhitespaceRe.Replace($"{personName.Name}.{personName.Surname}@example.com", "")
            )
        ).ToImmutableArray<IPerson>();
    }

    private record Person(string DisplayName, string EmailAddress) : IPerson;
}
