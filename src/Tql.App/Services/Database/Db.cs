using System.Data;
using System.Data.SQLite;
using System.IO;
using Dapper;
using Tql.Abstractions;
using Tql.Utilities;
using Path = System.IO.Path;

namespace Tql.App.Services.Database;

internal partial class Db : IDb, IDisposable
{
    private readonly IStore _store;
    private readonly SQLiteConnection _connection;

    private readonly AsyncLock _lock = new();

    public event EventHandler? TransactionCommitted;

    public Db(IStore store)
    {
        _store = store;

        SetupDapper();

        var fileName = GetDatabaseFileName();
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

    private string GetDatabaseFileName()
    {
        return Path.Combine(((Store)_store).DataFolder, "Tql.db");
    }

    private void SetupDapper()
    {
        SqlMapper.AddTypeHandler(new SQLiteGuidTypeHandler());
        SqlMapper.RemoveTypeMap(typeof(Guid));
        SqlMapper.RemoveTypeMap(typeof(Guid?));
    }

    private class SQLiteGuidTypeHandler : SqlMapper.TypeHandler<Guid>
    {
        public override void SetValue(IDbDataParameter parameter, Guid value)
        {
            parameter.Value = value.ToByteArray();
        }

        public override Guid Parse(object value)
        {
            return value switch
            {
                byte[] byteArrayValue => new Guid(byteArrayValue),
                string stringValue => Guid.Parse(stringValue),
                _
                    => throw new ArgumentException(
                        $"Cannot convert type '{value.GetType()}' to a Guid"
                    )
            };
        }
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
            var resourceName in GetType()
                .Assembly.GetManifestResourceNames()
                .Where(p =>
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
        var @lock = _lock.Lock();

        return new DbAccess(this, @lock);
    }

    public void Backup(Stream stream)
    {
        using (_lock.Lock())
        {
            var tempFileName = Path.GetTempFileName();

            try
            {
                using (var target = new SQLiteConnection($"data source={tempFileName}"))
                {
                    target.Open();

                    _connection.BackupDatabase(target, "main", "main", -1, null, -1);

                    ExecuteCommand("delete from Cache");
                    ExecuteCommand("vacuum");

                    void ExecuteCommand(string commandText)
                    {
                        using var command = target.CreateCommand();

                        command.CommandText = commandText;
                        command.ExecuteNonQuery();
                    }
                }

                using (var source = File.OpenRead(tempFileName))
                {
                    source.CopyTo(stream);
                }
            }
            finally
            {
                if (File.Exists(tempFileName))
                    File.Delete(tempFileName);
            }
        }
    }

    public void Restore(Stream stream)
    {
        var targetFileName = GetDatabaseFileName();

        foreach (
            var fileName in Directory.GetFiles(
                Path.GetDirectoryName(targetFileName)!,
                Path.GetFileName(targetFileName) + "*"
            )
        )
        {
            File.Delete(fileName);
        }

        using var target = File.Create(targetFileName);

        stream.CopyTo(target);
    }

    protected virtual void OnTransactionCommitted() =>
        TransactionCommitted?.Invoke(this, EventArgs.Empty);

    public void Dispose()
    {
        _lock.Dispose();

        _connection.Dispose();
    }
}
