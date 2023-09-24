namespace Launcher.App.Services.Database;

internal interface IDbAccess : IDisposable
{
    CacheEntity? GetCache(string key);
    void SetCache(CacheEntity entity);
    void DeleteCache(string key);
}
