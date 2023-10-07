using System.Windows.Media.Animation;

namespace Launcher.App.Support;

internal class MarqueeControl : ContentControl
{
    public static readonly DependencyProperty IsRunningProperty = DependencyProperty.Register(
        nameof(IsRunning),
        typeof(bool),
        typeof(MarqueeControl),
        new FrameworkPropertyMetadata(false, (d, e) => ((MarqueeControl)d).OnIsRunningChanged(d, e))
    );

    private Border? _border;
    private Canvas? _canvas;
    private ContentControl? _content;

    public bool IsRunning
    {
        get => (bool)GetValue(IsRunningProperty);
        set => SetValue(IsRunningProperty, value);
    }

    public MarqueeControl()
    {
        Loaded += MarqueeControl_Loaded;
        SizeChanged += MarqueeControl_SizeChanged;
    }

    private void MarqueeControl_Loaded(object sender, RoutedEventArgs e)
    {
        _border = this.FindVisualChild<Border>("_border")!;
        _canvas = this.FindVisualChild<Canvas>("_canvas")!;
        _content = this.FindVisualChild<ContentControl>("_content")!;

        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (_content == null || _border == null)
            return;

        var to = _border.ActualWidth - _content.ActualWidth;

        if (IsRunning && to < 0)
        {
            var duration = TimeSpan.FromMilliseconds(-to * 10);
            var delay = TimeSpan.FromSeconds(0.7);
            var wait = TimeSpan.FromSeconds(1);

            var animation = new DoubleAnimationUsingKeyFrames
            {
                RepeatBehavior = RepeatBehavior.Forever,
                KeyFrames =
                {
                    new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.Zero)),
                    new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(delay)),
                    new LinearDoubleKeyFrame(to, KeyTime.FromTimeSpan(delay + duration)),
                    new LinearDoubleKeyFrame(to, KeyTime.FromTimeSpan(delay + duration + wait))
                }
            };

            _content.BeginAnimation(Canvas.LeftProperty, animation);
            return;
        }

        _content.BeginAnimation(Canvas.LeftProperty, null);

        Canvas.SetLeft(_content, 0);
    }

    private void MarqueeControl_SizeChanged(object sender, SizeChangedEventArgs e) =>
        UpdateAnimation();

    private void OnIsRunningChanged(object sender, DependencyPropertyChangedEventArgs e) =>
        UpdateAnimation();
}
