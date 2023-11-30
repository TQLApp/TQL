using System.Globalization;

namespace Tql.App.Support;

internal class InvertedBooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool boolValue)
            boolValue = false;

        return boolValue ? Visibility.Collapsed : Visibility.Visible;
    }

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
