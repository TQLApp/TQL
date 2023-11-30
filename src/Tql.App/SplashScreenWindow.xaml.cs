using System.Collections;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Tql.Utilities;

namespace Tql.App;

internal partial class SplashScreenWindow
{
    private int _index;
    private readonly List<ImageResource> _images;
    private readonly DispatcherTimer _timer;

    public SplashScreenWindow()
    {
        InitializeComponent();

        var random = new Random();
        _images = LoadImages().OrderBy(_ => random.Next()).ToList();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += _timer_Tick;
        _timer.Start();

        _image1.Source = _image2.Source = GetNextImage();
    }

    private void _timer_Tick(object? sender, EventArgs e)
    {
        _image1.Source = _image2.Source;

        Canvas.SetLeft(_image1, 0);
        Canvas.SetLeft(_image2, _image1.Width);

        _image2.Source = GetNextImage();

        var animation1 = new DoubleAnimation
        {
            From = 0,
            To = -(_image1.Width + 5),
            Duration = TimeSpan.FromSeconds(0.4)
        };

        _image1.BeginAnimation(Canvas.LeftProperty, animation1);

        var animation2 = new DoubleAnimation
        {
            From = _image1.Width + 5,
            To = 0,
            Duration = TimeSpan.FromSeconds(0.4)
        };

        _image2.BeginAnimation(Canvas.LeftProperty, animation2);
    }

    private DrawingImage GetNextImage()
    {
        var imageResource = _images[_index++ % _images.Count];

        imageResource.LoadedImage ??= ImageFactory.CreateSvgImage(imageResource.Stream);

        return imageResource.LoadedImage;
    }

    private IEnumerable<ImageResource> LoadImages()
    {
        var resourceManager = new ResourceManager(
            $"{GetType().Assembly.GetName().Name}.g",
            GetType().Assembly
        );
        foreach (
            var resource in resourceManager
                .GetResourceSet(CultureInfo.CurrentUICulture, true, true)!
                .Cast<DictionaryEntry>()
        )
        {
            var resourceName = (string)resource.Key;
            var resourceStream = (Stream)resource.Value!;

            if (resourceName.Contains("/universe%20icons/", StringComparison.OrdinalIgnoreCase))
                yield return new ImageResource(resourceStream);
        }
    }

    private record ImageResource(Stream Stream)
    {
        public DrawingImage? LoadedImage { get; set; }
    }

    public void SetProgress(string? status, double progress)
    {
        _progressBar.Value = progress;
    }
}
