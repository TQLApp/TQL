namespace Tql.App.Support;

internal static class LabeledValue
{
    public static LabeledValue<T> Create<T>(string label, T value) => new(label, value);

    public static T GetValue<T>(object value) => ((LabeledValue<T>)value).Value;
}

internal class LabeledValue<T>(string label, T value)
{
    public T Value { get; } = value;

    public override string ToString() => label;

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj)
            || (obj is LabeledValue<T> other && Equals(Value, other.Value));
    }

    public override int GetHashCode()
    {
        return Value != null ? Value.GetHashCode() : 0;
    }
}
