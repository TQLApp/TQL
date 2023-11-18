namespace Tql.Abstractions;

/// <summary>
/// Represents a person in a <see cref="IPeopleDirectory"/>.
/// </summary>
public interface IPerson
{
    /// <summary>
    /// Gets the display name of the person.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Gets the email address of the person.
    /// </summary>
    string EmailAddress { get; }
}
