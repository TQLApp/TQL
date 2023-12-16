using System.Data.SQLite;
using Dapper;

namespace Tql.App.Services.Database;

internal partial class Db
{
    private class DbAccess(Db owner, IDisposable @lock) : IDbAccess
    {
        private bool _disposed;
        private readonly SQLiteTransaction _transaction = owner._connection.BeginTransaction();

        public CacheEntity? GetCache(string key)
        {
            return Query<CacheEntity>("select * from Cache where Key = @key", new { key })
                .SingleOrDefault();
        }

        public void SetCache(CacheEntity entity)
        {
            Execute(
                "replace into Cache(Key, Value, Version, Updated) values (@Key, @Value, @Version, @Updated)",
                entity
            );
        }

        public void DeleteCache(string key)
        {
            Execute("delete from Cache where Key = @key", new { key });
        }

        public List<HistoryEntity> GetHistory(int days)
        {
            return Query<HistoryEntity>(
                    "select * from History where LastAccess >= @Since order by LastAccess desc",
                    new { Since = DateTime.UtcNow - TimeSpan.FromDays(days) }
                )
                .ToList();
        }

        public void AddHistory(HistoryEntity history)
        {
            Execute(
                """
                insert into History(PluginId, ParentTypeId, ParentJson, TypeId, Json, IsPinned, LastAccess, AccessCount)
                values (@PluginId, @ParentTypeId, @ParentJson, @TypeId, @Json, @IsPinned, @LastAccess, 1)
                """,
                new
                {
                    history.PluginId,
                    history.ParentTypeId,
                    history.ParentJson,
                    history.TypeId,
                    history.Json,
                    IsPinned = history.IsPinned.GetValueOrDefault() != 0 ? (int?)1 : null,
                    LastAccess = DateTime.UtcNow
                }
            );

            history.Id = owner._connection.LastInsertRowId;
        }

        public void UpdateHistory(long id, string json)
        {
            Execute(
                """
                update History set Json = @json where Id = @id
                """,
                new { id, json }
            );
        }

        public void MarkHistoryAsAccessed(long id, string? parentJson)
        {
            Execute(
                "update History set LastAccess = @LastAccess, ParentJson = @parentJson, AccessCount = AccessCount + 1 where Id = @id",
                new
                {
                    id,
                    LastAccess = DateTime.UtcNow,
                    parentJson
                }
            );
        }

        public long? FindHistory(Guid pluginId, Guid typeId, string json)
        {
            return Query<long?>(
                    "select Id from History where PluginId = @pluginId and TypeId = @typeId and Json = @json",
                    new
                    {
                        pluginId,
                        typeId,
                        json
                    }
                )
                .SingleOrDefault();
        }

        public void DeleteHistory(long id)
        {
            Execute("delete from History where Id = @id", new { id });
        }

        public void SetHistoryPinned(long id, bool pinned)
        {
            Execute(
                "update History set IsPinned = @IsPinned where Id = @id",
                new { id, IsPinned = pinned ? (int?)1 : null }
            );
        }

        private IEnumerable<T> Query<T>(string sql, object? param = null) =>
            owner._connection.Query<T>(sql, param, _transaction);

        private void Execute(string sql, object? param = null) =>
            owner._connection.Execute(sql, param, _transaction);

        public void Dispose()
        {
            if (!_disposed)
            {
                _transaction.Commit();
                _transaction.Dispose();

                @lock.Dispose();

                owner.OnTransactionCommitted();

                _disposed = true;
            }
        }
    }
}
