namespace Tql.App.Themes.Attached
{
    public static class ToolTipHeaderHelper
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.RegisterAttached(
                "Value",
                typeof(string),
                typeof(ToolTipHeaderHelper),
                new PropertyMetadata(string.Empty, null, (_, v) => v ?? string.Empty)
            );

        public static void SetValue(DependencyObject element, string value) =>
            element.SetValue(ValueProperty, value);

        public static string GetValue(DependencyObject element) =>
            (string)element.GetValue(ValueProperty);

        public static readonly DependencyProperty VisibilityProperty =
            DependencyProperty.RegisterAttached(
                "Visibility",
                typeof(Visibility),
                typeof(ToolTipHeaderHelper),
                new PropertyMetadata(Visibility.Collapsed)
            );

        public static void SetVisibility(DependencyObject element, Visibility value) =>
            element.SetValue(VisibilityProperty, value);

        public static Visibility GetVisibility(DependencyObject element) =>
            (Visibility)element.GetValue(VisibilityProperty);
    }
}
