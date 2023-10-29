using System.Globalization;

namespace Tql.App.Support;

internal class BooleanToVisibilityHiddenConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool boolValue)
            boolValue = false;

        return boolValue ? Visibility.Visible : Visibility.Hidden;
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
