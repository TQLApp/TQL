namespace Tql.Abstractions;

/// <summary>
/// Specifies the buttons to show on a dialog box.
/// </summary>
/// <remarks>
/// Values of this enum can be combined to show more than one
/// button.
/// </remarks>
[Flags]
public enum DialogCommonButtons
{
    /// <summary>
    /// Indicates no buttons.
    /// </summary>
    None = 0,

    /// <summary>
    /// Indicates the OK button.
    /// </summary>
    OK = 1,

    /// <summary>
    /// Indicates the Yes button.
    /// </summary>
    Yes = 2,

    /// <summary>
    /// Indicates the No button.
    /// </summary>
    No = 4,

    /// <summary>
    /// Indicates the Cancel button.
    /// </summary>
    Cancel = 8,

    /// <summary>
    /// Indicates the Retry button.
    /// </summary>
    Retry = 16,

    /// <summary>
    /// Indicates the Close button.
    /// </summary>
    Close = 32,
}
