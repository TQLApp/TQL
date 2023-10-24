using Tql.App.Services;

namespace Tql.App.Support;

internal partial class NotificationBarUserControl
{
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text),
        typeof(string),
        typeof(NotificationBarUserControl),
        new FrameworkPropertyMetadata(string.Empty)
    );

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public event EventHandler<UINotificationEventArgs>? Activated;
    public event EventHandler<UINotificationEventArgs>? Dismissed;

    public NotificationBarUserControl()
    {
        InitializeComponent();

        _close.Source = Images.Dismiss;

        _close.AttachOnClickHandler(() =>
        {
            if (DataContext is UINotification notification)
                OnDismissed(new UINotificationEventArgs(notification));
        });
    }

    private void _hyperlink_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is UINotification notification)
            OnActivated(new UINotificationEventArgs(notification));
    }

    protected virtual void OnActivated(UINotificationEventArgs e) => Activated?.Invoke(this, e);

    protected virtual void OnDismissed(UINotificationEventArgs e) => Dismissed?.Invoke(this, e);
}
