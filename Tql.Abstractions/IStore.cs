namespace Tql.Abstractions;

public interface IStore
{
    string GetCacheFolder(Guid pluginId);
}
