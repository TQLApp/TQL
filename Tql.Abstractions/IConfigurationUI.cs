namespace Tql.Abstractions;

public interface IConfigurationUI
{
    Task<SaveStatus> Save();
}
