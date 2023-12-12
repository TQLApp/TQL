using System.Globalization;

namespace Tql.App.Support;

internal class IconNameToDrawingConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var iconName = value as string;
        if (string.IsNullOrEmpty(iconName))
            return null;

        return Images.GetImage(iconName);
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
