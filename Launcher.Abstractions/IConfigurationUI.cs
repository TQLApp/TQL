namespace Launcher.Abstractions;

public interface IConfigurationUI
{
    bool IsValid { get; }

    string GetConfiguration();
}
