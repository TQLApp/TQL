namespace Tql.Abstractions;

public interface IConfigurationPage
{
    Guid PageId { get; }
    string Title { get; }
    ConfigurationPageMode PageMode { get; }

    void Initialize(IConfigurationPageContext context);
    Task<SaveStatus> Save();
}
