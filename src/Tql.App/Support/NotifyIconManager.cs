using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.App.Services.Profiles;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;
using Timer = System.Threading.Timer;

namespace Tql.App.Support;

internal class NotifyIconManager : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private NotifyIconState _state;
    private readonly Timer _timer;
    private int _updateIconIndex;
    private volatile Icon[] _icons;
    private volatile IProfileConfiguration _profile;

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

        _profile = ProfileManager.GetCurrentProfile();

        _icons = LoadIcons();

        _notifyIcon.Icon = _icons[0];
        SetNotificationIconText(Labels.ApplicationTitle);
        _notifyIcon.Visible = true;

        _notifyIcon.Click += (_, e) =>
        {
            if (e is MouseEventArgs { Button: MouseButtons.Left })
                OnClicked();
        };

        _timer = new Timer(TimerCallback);

        UpdateIconFromState();
    }

    public void Initialize(IServiceProvider services)
    {
        var profileManager = services.GetRequiredService<IProfileManager>();

        profileManager.CurrentProfileChanged += (_, _) =>
        {
            _profile = profileManager.CurrentProfile;

            _icons = LoadIcons();

            UpdateIconFromState();
        };
    }

    private void TimerCallback(object? state)
    {
        _notifyIcon.Icon = _icons[(++_updateIconIndex % (_icons.Length - 1)) + 1];
    }

    private Icon[] LoadIcons()
    {
        var icons = new List<Icon>();

        var bugOverlay = Images.GetImage("Bug Overlay.svg");

        for (var i = 0; i < 5; i++)
        {
            var overlays = new List<ImageSource>();
#if DEBUG
            overlays.Add(bugOverlay);
#endif

            if (i > 0)
                overlays.Add(Images.GetImage($"Arrow Overlay {i}.svg"));

            using var stream = IconBuilder.Build(_profile.Image, overlays.ToArray());

            icons.Add(new Icon(stream));
        }

        return icons.ToArray();
    }

    private void UpdateIconFromState()
    {
        switch (_state)
        {
            case NotifyIconState.Starting:
                SetNotificationIconText(Labels.NotifyIconManager_ApplicationIsStarting);
                _notifyIcon.Icon = _icons[0];
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                break;

            case NotifyIconState.Updating:
                SetNotificationIconText(Labels.NotifyIconManager_ApplicationIsUpdating);
                _updateIconIndex = 0;
                _notifyIcon.Icon = _icons[_updateIconIndex + 1];
                _timer.Change(TimeSpan.FromSeconds(0.4), TimeSpan.FromSeconds(0.4));
                break;

            default:
                SetNotificationIconText(Labels.ApplicationTitle);
                _notifyIcon.Icon = _icons[0];
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                break;
        }
    }

    private void SetNotificationIconText(string text)
    {
        if (_profile.Name != null)
            text = $"{text} [{_profile.Title}]";

        _notifyIcon.Text = text;
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
