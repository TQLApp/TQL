using System.Text.Json;

namespace Tql.Plugins.Demo.Services;

internal static class PersonNames
{
    public static PersonNamesDto Load()
    {
        using var stream = typeof(PersonNames).Assembly.GetManifestResourceStream(
            $"{typeof(PersonNames).FullName}.json"
        )!;

        return JsonSerializer.Deserialize<PersonNamesDto>(stream)!;
    }

    public static IEnumerable<(string Name, string Surname)> Generate() => Generate(new Random());

    public static IEnumerable<(string Name, string Surname)> Generate(Random random)
    {
        var dto = Load();
        var seen = new HashSet<(string, string)>();

        while (true)
        {
            var surname = dto.Surnames[random.Next(0, dto.Surnames.Length)];
            var name =
                random.Next() % 2 == 0
                    ? dto.MaleNames[random.Next(0, dto.MaleNames.Length)]
                    : dto.FemaleNames[random.Next(0, dto.FemaleNames.Length)];

            var fullName = (name, surname);
            if (seen.Add(fullName))
                yield return fullName;
        }

        // This iterator never returns on purpose. The idea is you use Take to
        // take the number of names you like.
        // ReSharper disable once IteratorNeverReturns
    }
}
