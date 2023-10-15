namespace Tql.App.Services.Database;

internal class CacheEntity
{
    public string? Key { get; set; }
    public string? Value { get; set; }
    public int? Version { get; set; }
    public DateTime? Updated { get; set; }
}
