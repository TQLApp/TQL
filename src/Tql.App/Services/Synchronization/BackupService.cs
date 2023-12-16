using System.IO;
using System.IO.Compression;
using Tql.Abstractions;
using Tql.App.Services.Database;
using Tql.App.Support;

namespace Tql.App.Services.Synchronization;

internal class BackupService(
    Settings settings,
    LocalSettings localSettings,
    IConfigurationManager configurationManager,
    IDb db
)
{
    public Backup CreateBackup()
    {
        return new Backup(
            new BackupMetadata(
                BackupMetadata.CurrentVersion,
                settings.UserId!,
                Encryption.Unprotect(localSettings.EncryptionKey!)
            ),
            GetConfigurationBackup(),
            GetDbBackup()
        );
    }

    private ImmutableArray<byte> GetConfigurationBackup()
    {
        using var stream = new MemoryStream();

        ((ConfigurationManager)configurationManager).SaveConfiguration(stream);

        return stream.ToImmutableArray();
    }

    private ImmutableArray<byte> GetDbBackup()
    {
        using var stream = new MemoryStream();

        ((Db)db).Backup(stream);

        return stream.ToImmutableArray();
    }
}

internal record Backup(
    BackupMetadata Metadata,
    ImmutableArray<byte> Configuration,
    ImmutableArray<byte> Database
)
{
    public static Backup Deserialize(Stream stream)
    {
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

        return new Backup(
            ReadJsonEntry<BackupMetadata>("metadata.json"),
            ReadBinaryEntry("configuration.json"),
            ReadBinaryEntry("tql.db")
        );

        T ReadJsonEntry<T>(string name)
        {
            var entry = archive.GetEntry(name);
            if (entry == null)
                throw new BackupException($"Cannot find entry '{name}'");

            using var source = entry.Open();

            return JsonSerializer.Deserialize<T>(source)!;
        }

        ImmutableArray<byte> ReadBinaryEntry(string name)
        {
            var entry = archive.GetEntry(name);
            if (entry == null)
                throw new BackupException($"Cannot find entry '{name}'");

            using var source = entry.Open();

            return source.ToImmutableArray();
        }
    }

    public void Serialize(Stream stream)
    {
        using var archive = new ZipArchive(stream, ZipArchiveMode.Create, true);

        WriteJsonEntry("metadata.json", Metadata);
        WriteBinaryEntry("configuration.json", Configuration);
        WriteBinaryEntry("tql.db", Database);

        void WriteJsonEntry(string name, object value)
        {
            var entry = archive.CreateEntry(name);

            using var target = entry.Open();

            JsonSerializer.Serialize(target, value);
        }

        void WriteBinaryEntry(string name, ImmutableArray<byte> data)
        {
            var entry = archive.CreateEntry(name);

            using var target = entry.Open();

            target.Write(data.AsSpan());
        }
    }
}

internal record BackupMetadata(int Version, string UserId, string EncryptionKey)
{
    public const int CurrentVersion = 1;
};
