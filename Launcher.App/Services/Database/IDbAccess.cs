namespace Launcher.App.Services.Database;

internal interface IDbAccess : IDisposable
{
    CacheEntity? GetCache(string key);
    void SetCache(CacheEntity entity);
    void DeleteCache(string key);
    List<HistoryEntity> GetHistory(int days);
    void AddHistory(HistoryEntity history);
    void MarkHistoryAsAccessed(long id);
    long? FindHistory(Guid pluginId, Guid? parentTypeId, Guid typeId, string json);
    void DeleteHistory(long id);
}
