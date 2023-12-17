namespace Tql.App.Services.Database;

internal interface IDbAccess : IDisposable
{
    CacheEntity? GetCache(string key);
    void SetCache(CacheEntity entity);
    void DeleteCache(string key);
    List<HistoryEntity> GetHistory(int days);
    void AddHistory(HistoryEntity history);
    void UpdateHistory(long id, string json);
    void MarkHistoryAsAccessed(long id, string? parentJson);
    long? FindHistory(Guid pluginId, Guid typeId, string json);
    void DeleteHistory(long id);
    void SetHistoryPinned(long id, bool pinned);
}
