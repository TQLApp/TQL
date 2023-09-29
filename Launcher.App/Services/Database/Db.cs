using Dapper;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading;

namespace Launcher.App.Services.Database;

internal class Db : IDb, IDisposable
{
    private readonly SQLiteConnection _connection;

    // Using a semaphore instead of an object and Monitor.Exit/Leave
    // to support cross thread acquire and release.
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public Db(IStore store)
    {
        var fileName = Path.Combine(store.UserSettingsFolder, "Launcher.db");
        bool exists = File.Exists(fileName);

        _connection = new SQLiteConnection($"data source={fileName}");
        _connection.Open();

        Configure("foreign_keys", "on");
        Configure("temp_store", "memory");
        Configure("journal_mode", "wal");
        Configure("locking_mode", "exclusive");
        Configure("cache_size", "500");

        void Configure(string name, string value)
        {
            using var command = _connection.CreateCommand();

            command.CommandText = $"pragma {name}({value})";
            command.ExecuteNonQuery();
        }

        Migrate(!exists);
    }

    private void Migrate(bool isNew)
    {
        var migrations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!isNew)
        {
            migrations.UnionWith(
                _connection
                    .Query<MigrationEntity>("select Name from Migration")
                    .Select(p => p.Name!)
            );
        }

        var resourceNamePrefix = $"{GetType().Namespace}.Migrations.";

        foreach (
            var resourceName in GetType().Assembly
                .GetManifestResourceNames()
                .Where(
                    p =>
                        p.StartsWith(resourceNamePrefix)
                        && p.EndsWith(".sql", StringComparison.OrdinalIgnoreCase)
                )
                .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
        )
        {
            var migrationName = resourceName.Substring(resourceNamePrefix.Length);
            if (!migrations.Add(migrationName))
                continue;

            using var stream = GetType().Assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream!);

            using var command = _connection.CreateCommand();
            {
                command.CommandText = reader.ReadToEnd();

                command.ExecuteNonQuery();
            }

            _connection.Execute(
                "insert into Migration(Name) values (@Name)",
                new { Name = migrationName }
            );
        }
    }

    public IDbAccess Access()
    {
        _semaphore.Wait();

        return new DbAccess(this);
    }

    public void Dispose()
    {
        _semaphore.Dispose();

        _connection.Dispose();
    }

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
