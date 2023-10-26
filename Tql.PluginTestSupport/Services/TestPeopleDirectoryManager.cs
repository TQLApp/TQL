using Tql.Abstractions;

namespace Tql.PluginTestSupport.Services;

internal class TestPeopleDirectoryManager : IPeopleDirectoryManager
{
    private readonly List<IPeopleDirectory> _directories = new();
    private readonly object _syncRoot = new();

    public ImmutableArray<IPeopleDirectory> Directories
    {
        get
        {
            lock (_syncRoot)
            {
                return _directories.ToImmutableArray();
            }
        }
    }

    public void Add(IPeopleDirectory directory)
    {
        lock (_syncRoot)
        {
            _directories.Add(directory);
        }
    }

    public void Remove(IPeopleDirectory directory)
    {
        lock (_syncRoot)
        {
            _directories.Remove(directory);
        }
    }
}
