using Tql.Abstractions;

namespace Tql.App.Services;

internal class PeopleDirectoryManager : IPeopleDirectoryManager
{
    private volatile List<IPeopleDirectory> _directories = new();
    private readonly object _syncRoot = new();

    // _directories uses the safe publication pattern. The
    // list itself will never be updated.
    // ReSharper disable once InconsistentlySynchronizedField
    public ImmutableArray<IPeopleDirectory> Directories => _directories.ToImmutableArray();

    public event EventHandler? DirectoriesChanged;

    public void Add(IPeopleDirectory directory)
    {
        lock (_syncRoot)
        {
            var directories = _directories.ToList();

            directories.RemoveAll(p => p.Id == directory.Id);
            directories.Add(directory);

            _directories = directories;
        }

        OnDirectoriesChanged();
    }

    public void Remove(IPeopleDirectory directory)
    {
        lock (_syncRoot)
        {
            var directories = _directories.ToList();

            if (directories.Remove(directory))
                _directories = directories;
        }

        OnDirectoriesChanged();
    }

    protected virtual void OnDirectoriesChanged() =>
        DirectoriesChanged?.Invoke(this, EventArgs.Empty);
}
