using Tql.App.Support;

namespace Tql.App;

internal partial class SplashScreenWindow
{
    public SplashScreenWindow()
    {
        InitializeComponent();

        _progressBar.Value = 0;

        var slideShow = new SlideShow(_canvas);

        Closed += (_, _) => slideShow.Dispose();
    }

    public void SetProgress(string? status, double progress)
    {
        if (status != null)
            _label.Text = status;

        _progressBar.Value = progress;
    }
}
