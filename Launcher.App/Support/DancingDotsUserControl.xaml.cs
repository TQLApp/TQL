using System.Windows.Media.Animation;

namespace Launcher.App.Support;

internal partial class DancingDotsUserControl
{
    private Storyboard? _storyboard;

    public DancingDotsUserControl()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        var duration = TimeSpan.FromSeconds(0.14);

        var storyboard = new Storyboard { RepeatBehavior = RepeatBehavior.Forever };

        var offset = TimeSpan.Zero;

        foreach (var target in new[] { _image1, _image2, _image1, _image3 })
        {
            var result = new DoubleAnimationUsingKeyFrames
            {
                KeyFrames =
                {
                    new LinearDoubleKeyFrame(target.Height, KeyTime.FromTimeSpan(offset)),
                    new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(offset + duration)),
                    new LinearDoubleKeyFrame(
                        target.Height,
                        KeyTime.FromTimeSpan(offset + duration + duration)
                    )
                }
            };

            Storyboard.SetTarget(result, target);
            Storyboard.SetTargetProperty(result, new PropertyPath(HeightProperty));

            storyboard.Children.Add(result);

            offset += duration + duration;
        }

        _storyboard = storyboard;

        storyboard.Begin();
    }
}
