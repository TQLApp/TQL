using Dasync.Collections;
using Tql.Abstractions;
using Tql.Plugins.Demo.Services;

namespace Tql.Plugins.Demo.Test;

[TestFixture]
internal class PerformanceTestFixture : BaseFixture
{
    private ImmutableArray<PersonMatch> _people;

    public override void OneTimeSetUp()
    {
        base.OneTimeSetUp();

        _people = PersonNames
            .Generate(new Random(42))
            .Take(45_000)
            .Select(p => new PersonMatch($"{p.Surname}, {p.Name}"))
            .ToImmutableArray();
    }

    [TestCase("")]
    [TestCase("s")]
    [TestCase("se")]
    [TestCase("sea")]
    [TestCase("sean")]
    [TestCase("rsean")]
    [TestCase("rasean")]
    [TestCase("raysean")]
    [TestCase("rayseani")]
    [TestCase("rayseanic")]
    [TestCase("rayseanich")]
    [TestCase("rayseanicho")]
    public async Task PerformanceTest(string search)
    {
        var matches = await CreateSearchContext(search).Filter(_people).ToListAsync();

        _ = matches.Count();
    }

    private class PersonMatch : IMatch
    {
        public string Text { get; }
        public ImageSource Icon => throw new NotSupportedException();
        public MatchTypeId TypeId => throw new NotSupportedException();

        public PersonMatch(string text)
        {
            Text = text;
        }
    }
}
