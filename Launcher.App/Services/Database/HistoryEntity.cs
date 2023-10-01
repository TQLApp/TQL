namespace Launcher.App.Services.Database;

internal class HistoryEntity
{
    public long? Id { get; set; }
    public Guid? PluginId { get; set; }
    public Guid? ParentTypeId { get; set; }
    public Guid? TypeId { get; set; }
    public string? Json { get; set; }
    public DateTime? LastAccess { get; set; }
    public int? AccessCount { get; set; }
}
