using System.Globalization;

namespace Tql.App.QuickStart;

internal class Playbook(Dictionary<string, PlaybookPage> pages) : IPlaybook
{
    public static Playbook Load()
    {
        var culture = CultureInfo.CurrentUICulture;
        var playbook = new Playbook(new Dictionary<string, PlaybookPage>());

        while (!Equals(culture, CultureInfo.InvariantCulture))
        {
            using var stream = typeof(Playbook).Assembly.GetManifestResourceStream(
                $"{typeof(Playbook).Namespace}.Playbook.{culture.TwoLetterISOLanguageName}.md"
            );

            if (stream != null)
                playbook.AddMissingEntries(PlaybookParser.Parse(stream));

            culture = culture.Parent;
        }

        using var fallbackStream = typeof(Playbook).Assembly.GetManifestResourceStream(
            $"{typeof(Playbook).Namespace}.Playbook.md"
        );

        playbook.AddMissingEntries(PlaybookParser.Parse(fallbackStream!));

        return playbook;
    }

    public PlaybookPage this[string id] => pages[id];

    public event EventHandler? Updated
    {
        add { }
        remove { }
    }

    private void AddMissingEntries(Dictionary<string, PlaybookPage> newPages)
    {
        foreach (var entry in newPages)
        {
            if (!pages.ContainsKey(entry.Key))
                pages[entry.Key] = entry.Value;
        }
    }
}
