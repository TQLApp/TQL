using System.Windows.Media.Animation;

namespace Tql.App.Support;

internal partial class DancingDotsUserControl
{
    private Storyboard? _storyboard;

    public DancingDotsUserControl()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object? sender, RoutedEventArgs e)
    {
        var duration = TimeSpan.FromSeconds(0.14);

        var storyboard = new Storyboard { RepeatBehavior = RepeatBehavior.Forever };

        var offset = TimeSpan.Zero;

        foreach (var target in new[] { _image1, _image2, _image1, _image3 })
        {
            Timeline result = new DoubleAnimationUsingKeyFrames
            {
                KeyFrames =
                {
                    new LinearDoubleKeyFrame(target.Height, KeyTime.FromTimeSpan(offset)),
                    new LinearDoubleKeyFrame(
                        target.Height / 2,
                        KeyTime.FromTimeSpan(offset + duration)
                    ),
                    new LinearDoubleKeyFrame(
                        target.Height,
                        KeyTime.FromTimeSpan(offset + duration + duration)
                    )
                }
            };

            Storyboard.SetTarget(result, target);
            Storyboard.SetTargetProperty(result, new PropertyPath(HeightProperty));

            storyboard.Children.Add(result);

            result = new ThicknessAnimationUsingKeyFrames
            {
                KeyFrames =
                {
                    new LinearThicknessKeyFrame(target.Margin, KeyTime.FromTimeSpan(offset)),
                    new LinearThicknessKeyFrame(
                        target.Margin with
                        {
                            Bottom = 0
                        },
                        KeyTime.FromTimeSpan(offset + duration)
                    ),
                    new LinearThicknessKeyFrame(
                        target.Margin,
                        KeyTime.FromTimeSpan(offset + duration + duration)
                    )
                }
            };

            Storyboard.SetTarget(result, target);
            Storyboard.SetTargetProperty(result, new PropertyPath(MarginProperty));

            storyboard.Children.Add(result);

            offset += duration + duration;
        }

        _storyboard = storyboard;

        storyboard.Begin();
    }
}
