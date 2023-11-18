namespace Tql.Abstractions;

/// <summary>
/// Specifies the icon to show on a dialog box.
/// </summary>
public enum DialogIcon
{
    /// <summary>
    /// Indicates no icon.
    /// </summary>
    None = 0,

    /// <summary>
    /// Indicates a warning icon.
    /// </summary>
    Warning = unchecked((ushort)-1),

    /// <summary>
    /// Indicates an error icon.
    /// </summary>
    Error = unchecked((ushort)-2),

    /// <summary>
    /// Indicates an information icon.
    /// </summary>
    Information = unchecked((ushort)-3),

    /// <summary>
    /// Indicates a shield icon.
    /// </summary>
    Shield = unchecked((ushort)-4)
}
