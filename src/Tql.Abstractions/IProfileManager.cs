namespace Tql.Abstractions;

/// <summary>
/// Represents the profile manager.
/// </summary>
public interface IProfileManager
{
    /// <summary>
    /// Gets the configuration for the currently loaded profile.
    /// </summary>
    IProfileConfiguration CurrentProfile { get; }

    /// <summary>
    /// Occurs when the configuration of the current profile changes/
    /// </summary>
    public event EventHandler CurrentProfileChanged;
}

/// <summary>
/// Represents the configuration of a profile.
/// </summary>
public interface IProfileConfiguration
{
    /// <summary>
    /// Gets the environment name or <c>null</c> for the default profile.
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// Gets the user specified title.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Gets the image associated with the profile.
    /// </summary>
    ImageSource Image { get; }

    /// <summary>
    /// Gets the icon associated with the profile.
    /// </summary>
    ImageSource Icon { get; }
}
