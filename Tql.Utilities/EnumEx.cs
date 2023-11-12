namespace Tql.Utilities;

public static class EnumEx
{
    public static IEnumerable<T> GetValues<T>() => Enum.GetValues(typeof(T)).Cast<T>();
}
