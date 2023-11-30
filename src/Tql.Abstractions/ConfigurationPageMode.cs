namespace Tql.Abstractions;

/// <summary>
/// Identifies the scrolling mode of a configuration page.
/// </summary>
public enum ConfigurationPageMode
{
    /// <summary>
    /// Indicates that the page should fill the available space
    /// of the configuration window.
    /// </summary>
    /// <remarks>
    /// Use this if you e.g. are creating a configuration page to
    /// manage connections.
    /// </remarks>
    AutoSize,

    /// <summary>
    /// Indicates that the page should scroll vertically if the
    /// height exceeds the available space.
    /// </summary>
    /// <remarks>
    /// Use this if you e.g. are creating a page with a long list
    /// of settings.
    /// </remarks>
    Scroll
}
