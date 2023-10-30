using System.Diagnostics;
using System.Windows.Media;
using Tql.Abstractions;
using Tql.Plugins.Demo.Services;
using Tql.PluginTestSupport;

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
    public void PerformanceTest(string search)
    {
        var stopwatch = Stopwatch.StartNew();
        int count = CreateSearchContext(search).Filter(_people).Count();
        File.AppendAllText(
            @"c:\users\pvgin\downloads\log.txt",
            $"{search}: {count} {stopwatch.ElapsedMilliseconds}\r\n"
        );
    }

    private class PersonMatch : IMatch
    {
        private static readonly MatchTypeId MatchTypeId =
            new(
                Guid.Parse("7f3051ae-9784-4eb7-8bf9-9e7ab2f67eb8"),
                Guid.Parse("d7e702ae-89a8-43c4-bfd6-1cd2b0f7c627")
            );

        public string Text { get; }
        public ImageSource Icon => null!;
        public MatchTypeId TypeId => MatchTypeId;

        public PersonMatch(string text)
        {
            Text = text;
        }
    }
}
