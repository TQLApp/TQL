using System.Globalization;

namespace Tql.App.QuickStart;

internal class Playbook : IPlaybook
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

    private readonly Dictionary<string, PlaybookPage> _pages;

    public PlaybookPage this[string id] => _pages[id];

    public event EventHandler? Updated
    {
        add { }
        remove { }
    }

    public Playbook(Dictionary<string, PlaybookPage> pages)
    {
        _pages = pages;
    }

    private void AddMissingEntries(Dictionary<string, PlaybookPage> pages)
    {
        foreach (var entry in pages)
        {
            if (!_pages.ContainsKey(entry.Key))
                _pages[entry.Key] = entry.Value;
        }
    }
}
