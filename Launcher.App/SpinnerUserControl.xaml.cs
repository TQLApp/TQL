using Launcher.App.Services;

namespace Launcher.App;

internal partial class SpinnerUserControl
{
    private static readonly BitmapImage Spinner = LoadImage();

    private static BitmapImage LoadImage()
    {
        using var stream = typeof(SpinnerUserControl).Assembly.GetManifestResourceStream(
            $"{typeof(SpinnerUserControl).Namespace}.Resources.Spinner.png"
        );

        return ImageFactory.CreateBitmapImage(stream!);
    }

    public SpinnerUserControl()
    {
        InitializeComponent();

        _image.Source = Spinner;
    }
}
