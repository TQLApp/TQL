using System.Windows.Forms;
using System.Windows.Interop;
using Tql.Abstractions;
using Tql.App.Services;
using Tql.App.Support;
using Tql.Interop;
using Tql.Utilities;

namespace Tql.App.QuickStart;

internal class QuickStartManager
{
    private readonly Settings _settings;
    private readonly UI _ui;
    private QuickStartDto _state;
    private QuickStartWindow? _window;

    public QuickStartDto State
    {
        get => _state;
        set
        {
            if (_state != value)
            {
                _state = value;
                _settings.QuickStart = value.ToJson();
            }
        }
    }

    public QuickStartManager(Settings settings, IUI ui)
    {
        _settings = settings;
        _ui = (UI)ui;
        _state = QuickStartDto.FromSettings(settings);

        settings.AttachPropertyChanged(
            nameof(settings.QuickStart),
            (_, _) => _state = QuickStartDto.FromSettings(settings)
        );
    }

    public IDisposable Show(
        FrameworkElement owner,
        QuickStartPopup popup,
        QuickStartPopupMode mode = QuickStartPopupMode.None
    )
    {
        _window?.Close();

        var ownerWindow =
            Window.GetWindow(owner) ?? throw new InvalidOperationException("Cannot resolve window");
        var ownerIsControl = ownerWindow != owner;

        _window = new QuickStartWindow(_ui)
        {
            DataContext = popup,
            ArrowVisibility = ownerIsControl ? Visibility.Visible : Visibility.Collapsed
        };

        if (ownerIsControl)
            _window.ContentWidth *= .8;

        if (!mode.HasFlag(QuickStartPopupMode.Modal))
            _window.ShowActivated = false;

        _window.ChoiceButtonClicked += (_, e) =>
        {
            _window.Close();
            e.Button.Action();
        };

        _window.ButtonClicked += (_, e) =>
        {
            _window.Close();
            e.Button.Action();
        };

        _window.DismissClicked += (_, _) =>
        {
            var result = _ui.ShowConfirmation(
                _window,
                "Are you sure?",
                "Are you sure you want to close the Quick Start guide? You can restart the Quick Start guide from the settings screen at any time."
            );

            if (result == DialogResult.Yes)
            {
                _window.Close();
                State = State with { Step = QuickStartStep.Dismissed };
            }
        };

        _window.Owner = ownerWindow;

        _window.SourceInitialized += (_, _) => UpdateWindowLocation(owner, popup, mode);
        ownerWindow.LocationChanged += (_, _) => UpdateWindowLocation(owner, popup, mode);
        ownerWindow.SizeChanged += (_, _) => UpdateWindowLocation(owner, popup, mode);

        _window.Closed += (s, _) =>
        {
            if (_window == (Window)s)
                _window = null;
        };

        if (mode.HasFlag(QuickStartPopupMode.Modal))
        {
            _ui.EnterModalDialog();
            try
            {
                _window.ShowDialog();
            }
            finally
            {
                _ui.ExitModalDialog();
            }
        }
        else
        {
            _window.Show();
        }

        return new WindowCloser(_window);
    }

    private void UpdateWindowLocation(
        FrameworkElement owner,
        QuickStartPopup popup,
        QuickStartPopupMode mode = QuickStartPopupMode.None
    )
    {
        if (_window == null || !ReferenceEquals(_window.DataContext, popup))
            return;

        var ownerWindow =
            Window.GetWindow(owner) ?? throw new InvalidOperationException("Cannot resolve window");
        var ownerIsControl = ownerWindow != owner;

        // Get all bounds and sizes, and the screen we're working with,
        // adjusted for DPI scaling.
        var source = PresentationSource.FromVisual(ownerWindow)!;

        var scaleX = source.CompositionTarget!.TransformToDevice.M11;
        var scaleY = source.CompositionTarget!.TransformToDevice.M22;

        var ownerBounds = ScaleBounds(GetOwnerBounds());
        var windowSize = ScaleSize(new Size(_window.ActualWidth, _window.ActualHeight));
        var showOnScreen = ShowOnScreenManager.Create(_settings.ShowOnScreen);
        var screen = showOnScreen.GetScreen();

        // We base all random values on the hash code of the popup. This
        // means that the location of the popup will be stable, for a specific
        // popup.
        var random = new Random(popup.ToString().GetHashCode());

        // Calculate the desired offset off of the center of the owner window.
        var xRange = Math.Max(ownerBounds.Width - (windowSize.Width / 2), 0);
        var yRange = Math.Max(ownerBounds.Height - (windowSize.Height / 2), 0);
        var xOffset = random.Next(-(int)(xRange / 2), (int)(xRange / 2));
        var yOffset = random.Next(-(int)(yRange / 2), (int)(yRange / 2));

        // Adjust the offsets for the target window size.
        xOffset -= (int)windowSize.Width / 2;
        yOffset -= (int)windowSize.Height / 2;

        // Calculate the center of the owner window as the starting point.
        var x = ownerBounds.Left + ownerBounds.Width / 2;
        var y = ownerBounds.Top + ownerBounds.Height / 2;

        // Ensure the quick start window is always this amount of pixels from
        // the owner window and the screen border.
        var edgeDistance = 10 * scaleX;

        if (ownerIsControl)
        {
            x -= windowSize.Width / 2;
            y = ownerBounds.Bottom + edgeDistance;
        }
        else if (mode.HasFlag(QuickStartPopupMode.Modal))
        {
            // Randomly position the quick start window over the owning window.
            x += xOffset;
            y += yOffset;
        }
        else
        {
            // Put the quick start window to the side, at a random side.
            switch (random.Next(0, 4))
            {
                case 0: // North
                    x += xOffset;
                    y = ownerBounds.Top - (windowSize.Height + edgeDistance);
                    break;

                case 1: // East
                    x = ownerBounds.Right + edgeDistance;
                    y += yOffset;
                    break;

                case 2: // South
                    x += xOffset;
                    y = ownerBounds.Bottom + edgeDistance;
                    break;

                case 3: // West
                    x = ownerBounds.Left - (windowSize.Width + edgeDistance);
                    y += yOffset;
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        if (!ownerIsControl)
        {
            // Make sure the quick start window is visible on the screen.
            if (x < screen.Bounds.Left + edgeDistance)
                x = screen.Bounds.Left + edgeDistance;
            else if (x + windowSize.Width > screen.Bounds.Right - edgeDistance)
                x = screen.Bounds.Right - edgeDistance - windowSize.Width;
            if (y < screen.Bounds.Top + edgeDistance)
                y = screen.Bounds.Top + edgeDistance;
            else if (y + windowSize.Height > screen.Bounds.Bottom - edgeDistance)
                y = screen.Bounds.Bottom - edgeDistance - windowSize.Height;
        }

        _window.Left = x / scaleX;
        _window.Top = y / scaleY;

        Rect GetOwnerBounds()
        {
            if (ownerIsControl)
            {
                var location = owner.TransformToAncestor(ownerWindow).Transform(new Point());

                var clientRect = WindowInterop.GetClientRect(
                    new WindowInteropHelper(ownerWindow).Handle
                );

                return new Rect(
                    ownerWindow.Left + (clientRect.Left / scaleX) + location.X,
                    ownerWindow.Top + (clientRect.Top / scaleY) + location.Y,
                    owner.ActualWidth,
                    owner.ActualHeight
                );
            }

            return new Rect(
                ownerWindow.Left,
                ownerWindow.Top,
                ownerWindow.ActualWidth,
                ownerWindow.ActualHeight
            );
        }

        Rect ScaleBounds(Rect bounds) =>
            new(
                bounds.Left * scaleX,
                bounds.Top * scaleY,
                bounds.Width * scaleX,
                bounds.Height * scaleY
            );

        Size ScaleSize(Size size) => new(size.Width * scaleX, size.Height * scaleY);
    }

    public void Close()
    {
        _window?.Close();
    }

    private class WindowCloser(Window window) : IDisposable
    {
        public void Dispose()
        {
            window.Close();
        }
    }
}
