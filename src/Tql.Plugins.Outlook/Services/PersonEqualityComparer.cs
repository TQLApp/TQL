namespace Tql.Plugins.Outlook.Services;

internal class PersonEqualityComparer : IEqualityComparer<Person>
{
    public static readonly PersonEqualityComparer Instance = new();

    private PersonEqualityComparer() { }

    public bool Equals(Person? x, Person? y)
    {
        if (ReferenceEquals(x, y))
            return true;
        if (ReferenceEquals(x, null))
            return false;
        if (ReferenceEquals(y, null))
            return false;
        if (x.GetType() != y.GetType())
            return false;

        return string.Equals(
                x.DisplayName,
                y.DisplayName,
                StringComparison.CurrentCultureIgnoreCase
            )
            && string.Equals(
                x.EmailAddress,
                y.EmailAddress,
                StringComparison.CurrentCultureIgnoreCase
            );
    }

    public int GetHashCode(Person obj)
    {
        unchecked
        {
            return (StringComparer.CurrentCultureIgnoreCase.GetHashCode(obj.DisplayName) * 397)
                ^ StringComparer.CurrentCultureIgnoreCase.GetHashCode(obj.EmailAddress);
        }
    }
}
