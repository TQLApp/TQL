using System.Globalization;
using System.IO;
using Path = System.IO.Path;

namespace Tql.App.QuickStart;

internal class DebugPlaybook : IPlaybook, IDisposable
{
    private readonly string _path;

    public static IPlaybook Load()
    {
        return new DebugPlaybook(FindPlaybookPath());
    }

    private static string FindPlaybookPath()
    {
        var basePath = Path.GetDirectoryName(typeof(DebugPlaybook).Assembly.Location);
        var subPath = Path.Combine("Tql.App", "QuickStart");

        while (basePath != null)
        {
            var path = Path.Combine(basePath, subPath);
            if (File.Exists(Path.Combine(path, "Playbook.md")))
                return path;
            basePath = Path.GetDirectoryName(basePath);
        }

        throw new InvalidOperationException("Cannot find playbook path");
    }

    private readonly FileSystemWatcher _watcher;
    private readonly Dictionary<string, PlaybookPage> _pages = new();
    private readonly object _syncRoot = new();

    public PlaybookPage this[string id]
    {
        get
        {
            lock (_syncRoot)
            {
                return _pages[id];
            }
        }
    }

    public event EventHandler? Updated;

    private DebugPlaybook(string path)
    {
        _path = path;

        _watcher = new FileSystemWatcher { Path = path, };

        _watcher.Created += (_, _) => Load(true);
        _watcher.Changed += (_, _) => Load(true);
        _watcher.Deleted += (_, _) => Load(true);
        _watcher.Renamed += (_, _) => Load(true);

        _watcher.EnableRaisingEvents = true;

        Load(false);
    }

    private void Load(bool reload)
    {
        lock (_syncRoot)
        {
            _pages.Clear();

            var culture = CultureInfo.CurrentUICulture;

            while (!Equals(culture, CultureInfo.InvariantCulture))
            {
                var fileName = Path.Combine(
                    _path,
                    $"Playbook.{culture.TwoLetterISOLanguageName}.md"
                );
                if (File.Exists(fileName))
                    AddMissingEntries(fileName, reload);

                culture = culture.Parent;
            }

            AddMissingEntries(Path.Combine(_path, "Playbook.md"), reload);
        }

        OnUpdated();
    }

    private void AddMissingEntries(string fileName, bool reload)
    {
        for (var retry = 0; ; retry++)
        {
            try
            {
                // If the file is still being written to, this may fail.

                using var stream = new FileStream(
                    fileName,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read
                );

                var pages = PlaybookParser.Parse(stream);

                foreach (var entry in pages)
                {
                    if (!_pages.ContainsKey(entry.Key))
                        _pages.Add(entry.Key, entry.Value);
                }

                return;
            }
            catch when (reload && retry < 5)
            {
                Thread.Sleep(TimeSpan.FromSeconds(0.1));
            }
        }
    }

    protected virtual void OnUpdated() => Updated?.Invoke(this, EventArgs.Empty);

    public void Dispose()
    {
        _watcher.Dispose();
    }
}
