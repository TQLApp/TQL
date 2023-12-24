>>>>>CM:GREEN
using System.Collections.Immutable;
>>>>>CM:GREEN
using System.Composition;
>>>>>CM:GREEN
using System.Text.RegularExpressions;
>>>>>CM:GREEN
using Docfx.Plugins;

>>>>>CM:GREEN
namespace DocFxTqlPlugin;

>>>>>CM:GREEN
[Export(nameof(FixImageTags), typeof(IPostProcessor))]
>>>>>CM:GREEN
public class FixImageTags : IPostProcessor
>>>>>CM:GREEN
{
>>>>>CM:GREEN
    public ImmutableDictionary<string, object> PrepareMetadata(
>>>>>CM:GREEN
        ImmutableDictionary<string, object> metadata
>>>>>CM:GREEN
    )
>>>>>CM:GREEN
    {
>>>>>CM:GREEN
        return metadata;
>>>>>CM:GREEN
    }

>>>>>CM:GREEN
    public Manifest Process(Manifest manifest, string outputFolder)
>>>>>CM:GREEN
    {
>>>>>CM:GREEN
        foreach (
>>>>>CM:GREEN
            var fileName in Directory.GetFiles(outputFolder, "*.html", SearchOption.AllDirectories)
>>>>>CM:GREEN
        )
>>>>>CM:GREEN
        {
>>>>>CM:GREEN
            var content = File.ReadAllText(fileName);

>>>>>CM:GREEN
            var updated = Regex.Replace(
>>>>>CM:GREEN
                content,
>>>>>CM:GREEN
                """<img src="([^"]*)" alt="([^"]*)=(\d+x)">""",
>>>>>CM:GREEN
                Replace
>>>>>CM:GREEN
            );

>>>>>CM:GREEN
            if (content != updated)
>>>>>CM:GREEN
                File.WriteAllText(fileName, updated);
>>>>>CM:GREEN
        }

>>>>>CM:GREEN
        return manifest;
>>>>>CM:GREEN
    }

>>>>>CM:GREEN
    private string Replace(Match match)
>>>>>CM:GREEN
    {
>>>>>CM:GREEN
        var src = match.Groups[1].Value;
>>>>>CM:GREEN
        var alt = match.Groups[2].Value;
>>>>>CM:GREEN
        var size = match.Groups[3].Value;

>>>>>CM:GREEN
        return $"""<img src="{src}" srcset="{src} {size}" alt="{alt}">""";
>>>>>CM:GREEN
    }
>>>>>CM:GREEN
}
