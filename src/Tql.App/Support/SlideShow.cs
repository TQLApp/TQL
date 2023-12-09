using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Tql.App.Support;

internal class SlideShow : IDisposable
{
    private int _index;
    private readonly List<string> _images;
    private readonly DispatcherTimer _timer;
    private readonly Image _image1 = new();
    private readonly Image _image2 = new();

    public SlideShow(Canvas canvas)
    {
        var random = new Random();
        _images = Images.UniverseImages.OrderBy(_ => random.Next()).ToList();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += _timer_Tick;
        _timer.Start();

        _image1.Width = _image2.Width = canvas.Width;
        _image1.Height = _image2.Height = canvas.Height;

        canvas.Children.Add(_image1);
        canvas.Children.Add(_image2);

        _image2.Source = GetNextImage();

        SetStartingPosition();
    }

    private void _timer_Tick(object? sender, EventArgs e)
    {
        SetStartingPosition();

        _image2.Source = GetNextImage();

        _image1.BeginAnimation(
            Canvas.LeftProperty,
            new DoubleAnimation
            {
                From = 0,
                To = -(_image1.Width + 5),
                Duration = TimeSpan.FromSeconds(0.4)
            }
        );

        _image2.BeginAnimation(
            Canvas.LeftProperty,
            new DoubleAnimation
            {
                From = _image1.Width + 5,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.4)
            }
        );
    }

    private void SetStartingPosition()
    {
        _image1.Source = _image2.Source;

        Canvas.SetLeft(_image1, 0);
        Canvas.SetLeft(_image2, _image1.Width);
    }

    private DrawingImage GetNextImage()
    {
        return Images.GetImage(_images[_index++ % _images.Count]);
    }

    public void Dispose()
    {
        _timer.Stop();
    }
}
