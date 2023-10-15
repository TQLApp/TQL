using System.ComponentModel;
using System.Diagnostics;
using Tql.Setup.App.Support;
using Tql.Utilities;

namespace Tql.Setup.App;

internal partial class MainWindow
{
    private static DrawingImage LoadImage(string resourceName)
    {
        using var stream = Application
            .GetResourceStream(new Uri($"/Resources/{resourceName}", UriKind.Relative))!
            .Stream;

        return ImageFactory.CreateSvgImage(stream!);
    }

    public static readonly DrawingImage RunImage = LoadImage("Space Shuttle.svg");
    private Installer? _installer;

    public MainWindow()
    {
        InitializeComponent();

        SetVisible(_confirm);

        _icon.Source = RunImage;
    }

    private void _no_Click(object sender, RoutedEventArgs e) => Close();

    private void _yes_Click(object sender, RoutedEventArgs e)
    {
        SetVisible(_installation);

        _installer = new Installer();

        _installer.ProgressChanged += _installer_ProgressChanged;

        _installer.Start();
    }

    private void _installer_ProgressChanged(object sender, EventArgs e)
    {
        if (_installer == null)
            return;

        if (_installer.Exception != null)
        {
            TaskDialogEx.Error(this, "An unexpected error occurred", _installer.Exception);
            Close();
        }

        if (_installer.Done)
            SetVisible(_done);

        _progress.Value =
            _progress.Minimum + (_installer.Progress * (_progress.Maximum - _progress.Minimum));
    }

    private void _openDocumentation_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start("https://github.com/pvginkel/TQL#readme");
        }
        catch
        {
            // Ignore.
        }
    }

    private void _close_Click(object sender, RoutedEventArgs e) => Close();

    private void SetVisible(UIElement element)
    {
        _confirm.Visibility = GetVisibility(_confirm == element);
        _installation.Visibility = GetVisibility(_installation == element);
        _done.Visibility = GetVisibility(_done == element);

        Visibility GetVisibility(bool value) => value ? Visibility.Visible : Visibility.Collapsed;
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        if (_installer is { Done: false })
            e.Cancel = true;
    }
}
