using System.IO;
using YamlDotNet.Serialization;

namespace Tql.App.QuickStart;

internal static class PlaybookParser
{
    public static Dictionary<string, PlaybookPage> Parse(Stream stream)
    {
        using var reader = new StreamReader(stream);

        var content = reader.ReadToEnd();

        var pages = new Dictionary<string, PlaybookPage>();

        foreach (var block in GetBlocks(content))
        {
            var frontmatter = new Deserializer().Deserialize<Dictionary<string, string>>(
                block.Frontmatter
            );

            pages[frontmatter["id"]] = new PlaybookPage(
                frontmatter["id"],
                frontmatter["title"],
                block.Content.Trim()
            );
        }

        return pages;
    }

    private static IEnumerable<(string Frontmatter, string Content)> GetBlocks(string content)
    {
        var parts = content.Split(new[] { "---" }, StringSplitOptions.None);

        for (var i = 1; i < parts.Length; i += 2)
        {
            yield return (parts[i], parts[i + 1]);
        }
    }
}
