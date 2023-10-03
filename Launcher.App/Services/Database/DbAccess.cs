using System.Data.SQLite;
using Dapper;
using Microsoft.VisualStudio.Services.Operations;

namespace Launcher.App.Services.Database;

internal partial class Db
{
    private class DbAccess : IDbAccess
    {
        private readonly Db _owner;
        private bool _disposed;
        private readonly SQLiteTransaction _transaction;

        public DbAccess(Db owner)
        {
            _owner = owner;

            _transaction = owner._connection.BeginTransaction();
        }

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
                "insert into History(PluginId, ParentTypeId, TypeId, Json, LastAccess, AccessCount) values (@PluginId, @TypeId, @Json, @LastAccess, 1)",
                new
                {
                    history.PluginId,
                    history.ParentTypeId,
                    history.TypeId,
                    history.Json,
                    LastAccess = DateTime.UtcNow
                }
            );

            history.Id = _owner._connection.LastInsertRowId;
        }

        public void MarkHistoryAsAccessed(long id)
        {
            Execute(
                "update History set LastAccess = @LastAccess, AccessCount = AccessCount + 1",
                new { LastAccess = DateTime.UtcNow }
            );
        }

        public long? FindHistory(Guid? parentTypeId, Guid typeId, string json)
        {
            var sql = "select Id from History where ";
            if (parentTypeId.HasValue)
                sql += "ParentTypeId = @parentTypeId and ";
            else
                sql += "ParentTypeId is null and ";
            sql += "TypeId = @typeId and Json = @json";

            return Query<long?>(
                    sql,
                    new
                    {
                        parentTypeId,
                        typeId,
                        json
                    }
                )
                .Single();
        }

        public void DeleteHistory(long id)
        {
            Execute("delete from History where Id = @id", new { id });
        }

        private IEnumerable<T> Query<T>(string sql, object? param = null) =>
            _owner._connection.Query<T>(sql, param, _transaction);

        private void Execute(string sql, object? param = null) =>
            _owner._connection.Execute(sql, param, _transaction);

        public void Dispose()
        {
            if (!_disposed)
            {
                _transaction.Commit();
                _transaction.Dispose();

                _owner._semaphore.Release();

                _disposed = true;
            }
        }
    }
}
