namespace Launcher.Abstractions;

public interface IConfigurationUI
{
    string? GetValidationResult();

    void Save();
}
