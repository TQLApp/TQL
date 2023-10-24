using System.Data;
using System.Data.SQLite;
using System.IO;
using Dapper;
using Tql.Abstractions;
using Path = System.IO.Path;

namespace Tql.App.Services.Database;

internal partial class Db : IDb, IDisposable
{
    private readonly SQLiteConnection _connection;

    // Using a semaphore instead of an object and Monitor.Exit/Leave
    // to support cross thread acquire and release.
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public Db(IStore store)
    {
        SetupDapper();

        var fileName = Path.Combine(((Store)store).DataFolder, "Tql.db");
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
}
