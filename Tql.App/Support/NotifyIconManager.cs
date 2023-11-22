using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Application = System.Windows.Application;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;
using Timer = System.Threading.Timer;

namespace Tql.App.Support;

internal class NotifyIconManager : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private NotifyIconState _state;
    private readonly Timer _timer;
    private readonly List<Icon> _updateIcons;
    private int _updateIconIndex;
    private readonly Icon _icon;

    public ContextMenuStrip? ContextMenuStrip
    {
        get => _notifyIcon.ContextMenuStrip;
        set => _notifyIcon.ContextMenuStrip = value;
    }

    public NotifyIconState State
    {
        get => _state;
        set
        {
            if (_state != value)
            {
                _state = value;
                UpdateIconFromState();
            }
        }
    }

    public event EventHandler? Clicked;

    public NotifyIconManager()
    {
        _notifyIcon = new NotifyIcon();

        _icon = LoadIcon();

        _updateIcons = CreateUpdateIcons(_icon);

        _notifyIcon.Icon = _icon;
        _notifyIcon.Text = Labels.ApplicationTitle;
        _notifyIcon.Visible = true;

        _notifyIcon.Click += (_, e) =>
        {
            if (e is MouseEventArgs { Button: MouseButtons.Left })
                OnClicked();
        };

        _timer = new Timer(TimerCallback);

        UpdateIconFromState();
    }

    private void TimerCallback(object state)
    {
        _notifyIcon.Icon = _updateIcons[++_updateIconIndex % _updateIcons.Count];
    }

    private List<Icon> CreateUpdateIcons(Icon icon)
    {
        var taskbarScreenDpi = Win32Utils.GetTaskbarScreenDpi();

        var iconSize = taskbarScreenDpi.DpiX > 96 ? 32 : 16;

        var icons = new List<Icon>();

        using var arrow = LoadBitmap($"Arrow {iconSize}.png");

        using var iconBitmap = GetIconBitmap(icon, iconSize);

        for (var i = 0; i < 4; i++)
        {
            using var rotated = RotateBitmap(arrow, i);
            using var overlayed = OverlayBitmap(iconBitmap, rotated);

            icons.Add(Icon.FromHandle(overlayed.GetHicon()));
        }

        return icons;
    }

    private Bitmap OverlayBitmap(Bitmap icon, Bitmap bitmap)
    {
        var result = new Bitmap(icon.Width, icon.Height);

        using (var g = Graphics.FromImage(result))
        {
            g.InterpolationMode = InterpolationMode.NearestNeighbor;

            g.DrawImageUnscaled(icon, new Point());
            g.DrawImageUnscaled(
                bitmap,
                new Rectangle(
                    icon.Width - bitmap.Width,
                    icon.Height - bitmap.Height,
                    bitmap.Width,
                    bitmap.Height
                )
            );
        }

        return result;
    }

    private Bitmap RotateBitmap(Bitmap bitmap, int step)
    {
        var result = new Bitmap(bitmap.Width, bitmap.Height);

        bitmap.SetResolution(result.HorizontalResolution, result.VerticalResolution);
        result.MakeTransparent();

        var center = new Point(bitmap.Width / 2, bitmap.Height / 2);

        using (var g = Graphics.FromImage(result))
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            g.TranslateTransform(center.X, center.Y);
            g.RotateTransform(step * 45);
            g.TranslateTransform(-center.X, -center.Y);

            g.DrawImage(bitmap, new Point());
        }

        return result;
    }

    private Bitmap GetIconBitmap(Icon icon, int size)
    {
        var bitmap = new Bitmap(size, size);

        using (var g = Graphics.FromImage(bitmap))
        {
            g.InterpolationMode = InterpolationMode.NearestNeighbor;

            using var sizedIcon = new Icon(icon, size, size);

            g.DrawIconUnstretched(sizedIcon, new Rectangle(0, 0, size, size));
        }

        return bitmap;
    }

    private Icon LoadIcon()
    {
        using var stream = Application
            .GetResourceStream(new Uri("pack://application:,,,/Tql.App;component/mainicon.ico"))!
            .Stream;

        return new Icon(stream);
    }

    private Bitmap LoadBitmap(string name)
    {
        using var stream = Application
            .GetResourceStream(
                new Uri($"pack://application:,,,/Tql.App;component/Resources/{name}")
            )!
            .Stream;

        return new Bitmap(stream);
    }

    private void UpdateIconFromState()
    {
        switch (_state)
        {
            case NotifyIconState.Starting:
                _notifyIcon.Text = Labels.NotifyIconManager_ApplicationIsStarting;
                _notifyIcon.Icon = _icon;
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                break;

            case NotifyIconState.Updating:
                _notifyIcon.Text = Labels.NotifyIconManager_ApplicationIsUpdating;
                _updateIconIndex = 0;
                _notifyIcon.Icon = _updateIcons[_updateIconIndex];
                _timer.Change(TimeSpan.FromSeconds(0.4), TimeSpan.FromSeconds(0.4));
                break;

            default:
                _notifyIcon.Text = Labels.ApplicationTitle;
                _notifyIcon.Icon = _icon;
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                break;
        }
    }

    protected virtual void OnClicked() => Clicked?.Invoke(this, EventArgs.Empty);

    public void Dispose()
    {
        _notifyIcon.Dispose();
    }
}

internal enum NotifyIconState
{
    Starting,
    Updating,
    Running
}
