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

        self.MouseDown += (_, _) => self.CaptureMouse();

        self.MouseUp += (s, e) =>
        {
            if (e.ChangedButton != MouseButton.Left)
                return;

            if (self.IsMouseOver)
                action(s, e);

            self.ReleaseMouseCapture();

            e.Handled = true;
        };
    }
}
