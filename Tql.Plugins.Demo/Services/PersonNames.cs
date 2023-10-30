using Bogus;

namespace Tql.Plugins.Demo.Services;

internal static class PersonNames
{
    public static IEnumerable<Person> Generate() => Generate(new Random());

    public static IEnumerable<Person> Generate(Random random)
    {
        Randomizer.Seed = random;

        var faker = new Faker<Person>().CustomInstantiator(
            p => new Person(p.Name.FirstName(), p.Name.LastName())
        );

        var seen = new HashSet<Person>();

        foreach (var person in faker.GenerateForever())
        {
            if (seen.Add(person))
                yield return person;
        }
    }
}

internal record Person(string Name, string Surname);
