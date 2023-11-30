using System.Collections.Immutable;
using System.Composition;
using System.Text.RegularExpressions;
using Docfx.Plugins;

namespace DocFxTqlPlugin;

[Export(nameof(FixImageTags), typeof(IPostProcessor))]
public class FixImageTags : IPostProcessor
{
    public ImmutableDictionary<string, object> PrepareMetadata(
        ImmutableDictionary<string, object> metadata
    )
    {
        return metadata;
    }

    public Manifest Process(Manifest manifest, string outputFolder)
    {
        foreach (
            var fileName in Directory.GetFiles(outputFolder, "*.html", SearchOption.AllDirectories)
        )
        {
            var content = File.ReadAllText(fileName);

            var updated = Regex.Replace(
                content,
                """<img src="([^"]*)" alt="([^"]*)=(\d+x)">""",
                Replace
            );

            if (content != updated)
                File.WriteAllText(fileName, updated);
        }

        return manifest;
    }

    private string Replace(Match match)
    {
        var src = match.Groups[1].Value;
        var alt = match.Groups[2].Value;
        var size = match.Groups[3].Value;

        return $"""<img src="{src}" srcset="{src} {size}" alt="{alt}">""";
    }
}
