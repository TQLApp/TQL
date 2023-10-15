// From https://stackoverflow.com/questions/44695640

using System.Globalization;

namespace Tql.App.Support;

internal class HeightToRadiusConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double height)
            return height / 2;
        return value;
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        return new NotSupportedException();
    }
}
