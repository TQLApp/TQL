using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Bogus;
using Dasync.Collections;
using Tql.Abstractions;
using Tql.App.Search;

namespace Tql.Benchmarks;

[SimpleJob(RuntimeMoniker.Net481, warmupCount: 5, iterationCount: 20)]
public class FilterBenchmark
{
    private IMatch[] _people = null!;

    [field: Params(100, 1000, 50_000)]
    public int Count;

    [field: Params(
        "",
        "s",
        "se",
        "sea",
        "sean",
        "rsean",
        "rasean",
        "raysean",
        "rayseani",
        "rayseanic",
        "rayseanich",
        "rayseanicho"
    )]
    public string Search = null!;

    [GlobalSetup]
    public void Setup()
    {
        Randomizer.Seed = new Random(42);

        var faker = new Faker<PersonMatch>().CustomInstantiator(
            p => new PersonMatch(p.Name.FirstName(), p.Name.LastName())
        );

        _people = faker.Generate(Count).ToArray<IMatch>();
    }

    [Benchmark]
    public int Benchmark()
    {
        var context = new SearchContext(null!, Search, null, null!);

        return context.Filter(_people).Take(100).ToListAsync().Result.Count();
    }

    [DebuggerDisplay("Text = {Text}")]
    private class PersonMatch(string name, string lastName) : IMatch
    {
        public string Text { get; } = $"{lastName}, {name}";
        public ImageSource Icon => throw new NotSupportedException();
        public MatchTypeId TypeId => throw new NotSupportedException();
    }
}
