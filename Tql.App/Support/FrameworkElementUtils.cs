namespace Tql.App.Support;

internal static class FrameworkElementUtils
{
    public static void AttachOnClickHandler(this FrameworkElement self, Action action)
    {
        self.AttachOnClickHandler((_, _) => action());
    }

    public static void AttachOnClickHandler(
        this FrameworkElement self,
        Action<object, MouseButtonEventArgs> action
    )
    {
        self.Cursor = Cursors.Hand;

        self.MouseDown += (_, e) =>
        {
            e.Handled = true;

            self.CaptureMouse();
        };

        self.MouseUp += (s, e) =>
        {
            e.Handled = true;

            if (e.ChangedButton != MouseButton.Left)
                return;

            self.ReleaseMouseCapture();

            if (self.IsMouseDirectlyOver)
                action(s, e);

            e.Handled = true;
        };
    }
}
