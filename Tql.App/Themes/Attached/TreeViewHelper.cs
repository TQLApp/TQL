namespace Tql.App.Themes.Attached;

public static class TreeViewHelper
{
    public static readonly DependencyProperty ShowLinesProperty =
        DependencyProperty.RegisterAttached(
            "ShowLines",
            typeof(bool),
            typeof(TreeViewHelper),
            new PropertyMetadata(true)
        );

    public static void SetShowLines(DependencyObject element, bool value) =>
        element.SetValue(ShowLinesProperty, value);

    public static bool GetShowLines(DependencyObject element) =>
        (bool)element.GetValue(ShowLinesProperty);
}
