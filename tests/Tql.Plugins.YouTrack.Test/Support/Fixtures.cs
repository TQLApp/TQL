using System.IO;

namespace Tql.Plugins.YouTrack.Test.Support;

/// <summary>
/// The captured answers in Fixtures/, embedded in the test assembly.
/// </summary>
internal static class Fixtures
{
    public static string Read(string name)
    {
        var assembly = typeof(Fixtures).Assembly;

        using var stream =
            assembly.GetManifestResourceStream($"Fixtures.{name}")
            ?? throw new FileNotFoundException($"No fixture named '{name}'");
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }
}
