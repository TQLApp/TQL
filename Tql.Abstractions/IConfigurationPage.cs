namespace Tql.Abstractions;

public interface IConfigurationPage
{
    Guid PageId { get; }
    string Title { get; }

    Task<SaveStatus> Save();
}
