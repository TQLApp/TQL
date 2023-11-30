namespace Tql.Abstractions;

/// <summary>
/// Specifies the result of saving a configuration page. See
/// <see cref="IConfigurationPage.Save()"/>.
/// </summary>
public enum SaveStatus
{
    /// <summary>
    /// Indicates success. If all configuration pages indicate success,
    /// the configuration window will be closed.
    /// </summary>
    Success,

    /// <summary>
    /// Indicates failure. If any of the configuration pages indicate
    /// failure, the configuration window will not be closed.
    /// </summary>
    Failure
}
