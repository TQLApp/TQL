using System.Globalization;

namespace Tql.Utilities;

/// <summary>
/// Converter to convert null values to a boolean (null being false).
/// </summary>
public class NullToBooleanConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value != null;
    }

    /// <inheritdoc/>
    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        throw new NotSupportedException();
    }
}
