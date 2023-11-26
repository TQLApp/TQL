using System.Windows.Forms;
using System.Windows.Interop;
using Tql.Abstractions;
using Tql.App.Services;
using Tql.App.Support;
using Tql.Utilities;

namespace Tql.App.QuickStart;

internal class QuickStartManager
{
    private readonly Settings _settings;
    private readonly UI _ui;
    private QuickStartDto _state;
    private QuickStartWindow? _window;
    private bool _repositioning;

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

        var adorner = default(Adorner);

        if (ownerIsControl)
        {
            adorner = new QuickStartAdorner(owner);

            AdornerLayer.GetAdornerLayer(owner)!.Add(adorner);
        }

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

        _window.SourceInitialized += (_, _) => UpdateWindowLocation(owner, popup, mode, true);
        ownerWindow.LocationChanged += (_, _) => UpdateWindowLocation(owner, popup, mode, false);
        ownerWindow.SizeChanged += (_, _) => UpdateWindowLocation(owner, popup, mode, false);

        _window.Closed += (s, _) =>
        {
            if (adorner != null)
                AdornerLayer.GetAdornerLayer(owner)!.Remove(adorner);

            if (_window == (Window?)s)
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
        QuickStartPopupMode mode,
        bool initialUpdate
    )
    {
        if (_repositioning || _window == null || !ReferenceEquals(_window.DataContext, popup))
            return;

        _repositioning = true;

        try
        {
            DoUpdateWindowLocation(owner, popup, mode, initialUpdate);
        }
        finally
        {
            _repositioning = false;
        }
    }

    private void DoUpdateWindowLocation(
        FrameworkElement owner,
        QuickStartPopup popup,
        QuickStartPopupMode mode,
        bool initialUpdate
    )
    {
        var ownerWindow =
            Window.GetWindow(owner) ?? throw new InvalidOperationException("Cannot resolve window");
        var ownerIsControl = ownerWindow != owner;

        // Get all bounds and sizes, and the screen we're working with,
        // adjusted for DPI scaling.
        var source = PresentationSource.FromVisual(ownerWindow)!;

        var scaleX = source.CompositionTarget!.TransformToDevice.M11;
        var scaleY = source.CompositionTarget!.TransformToDevice.M22;

        var ownerBounds = ScaleBounds(GetOwnerBounds());
        var ownerWindowBounds = ScaleBounds(GetOwnerWindowBounds());
        var windowSize = ScaleSize(new Size(_window!.ActualWidth, _window.ActualHeight));
        var showOnScreen = ShowOnScreenManager.Create(_settings.ShowOnScreen);
        var workingArea = showOnScreen.GetScreen().WorkingArea;

        // We base all random values on the hash code of the ID of the popup. This
        // means that the location of the popup will be stable, for a specific
        // popup. We can't use GetHashCode for this because that's randomized.
        var random = new Random(popup.Id.GetStableHashCode());

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
        var repositionEdge = Edge.None;

        if (ownerIsControl)
        {
            x -= windowSize.Width / 2;
            y = ownerBounds.Bottom + (QuickStartAdorner.Distance - 1) * scaleY;

            // Ensure that the quick start window is visible.
            var bottom = y + windowSize.Height + edgeDistance;
            var overhang = bottom - workingArea.Bottom;
            if (overhang > 0)
            {
                var availableSpace = ownerWindowBounds.Top - (workingArea.Top + edgeDistance);
                if (overhang > availableSpace)
                    overhang = availableSpace;

                y -= overhang;
                ownerWindow.Top -= overhang / scaleY;
            }
        }
        else if (mode.HasFlag(QuickStartPopupMode.Modal))
        {
            // Randomly position the quick start window over the owning window.
            x += xOffset;
            y += yOffset;
        }
        else
        {
            var maxEdge = 4;

            // Prevent the quick start window from showing at the top of
            // bottom if it wouldn't fit.
            var requiredSpace = (windowSize.Height + ownerWindowBounds.Height + edgeDistance * 2);
            if (requiredSpace > workingArea.Height)
                maxEdge = 2;

            // Put the quick start window to the side, at a random side.
            repositionEdge = (Edge)random.Next(0, maxEdge);

            switch (repositionEdge)
            {
                case Edge.Left:
                    x = ownerBounds.Left - (windowSize.Width + edgeDistance);
                    y += yOffset;
                    break;

                case Edge.Right:
                    x = ownerBounds.Right + edgeDistance;
                    y += yOffset;
                    break;

                case Edge.Top:
                    x += xOffset;
                    y = ownerBounds.Top - (windowSize.Height + edgeDistance);
                    break;

                case Edge.Bottom:
                    x += xOffset;
                    y = ownerBounds.Bottom + edgeDistance;
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        if (!ownerIsControl)
        {
            // Make sure the quick start window is visible on the screen.
            if (x < workingArea.Left + edgeDistance)
                x = workingArea.Left + edgeDistance;
            else if (x + windowSize.Width > workingArea.Right - edgeDistance)
                x = workingArea.Right - edgeDistance - windowSize.Width;
            if (y < workingArea.Top + edgeDistance)
                y = workingArea.Top + edgeDistance;
            else if (y + windowSize.Height > workingArea.Bottom - edgeDistance)
                y = workingArea.Bottom - edgeDistance - windowSize.Height;
        }

        // Minimize overlap with the owner window.
        if (initialUpdate)
        {
            switch (repositionEdge)
            {
                case Edge.Left:
                    var right = x + windowSize.Width;
                    var overhang = right - ownerWindowBounds.Left;
                    if (overhang > 0)
                    {
                        var availableSpace =
                            workingArea.Right - ownerWindowBounds.Right - edgeDistance;
                        ownerWindow.Left += Math.Min(overhang, availableSpace);
                    }
                    break;

                case Edge.Right:
                    overhang = ownerWindowBounds.Right - x;
                    if (overhang > 0)
                    {
                        var availableSpace =
                            ownerWindowBounds.Left - workingArea.Left - edgeDistance;
                        ownerWindow.Left -= Math.Min(overhang, availableSpace);
                    }
                    break;

                case Edge.Top:
                    var bottom = y + windowSize.Height;
                    overhang = bottom - ownerWindowBounds.Top;
                    if (overhang > 0)
                    {
                        var availableSpace =
                            workingArea.Bottom - ownerWindowBounds.Bottom - edgeDistance;
                        ownerWindow.Top += Math.Min(overhang, availableSpace);
                    }
                    break;

                case Edge.Bottom:
                    overhang = ownerWindowBounds.Bottom - y;
                    if (overhang > 0)
                    {
                        var availableSpace = ownerWindowBounds.Top - workingArea.Top - edgeDistance;
                        ownerWindow.Top -= Math.Min(overhang, availableSpace);
                    }
                    break;
            }
        }

        _window.Left = x / scaleX;
        _window.Top = y / scaleY;

        Rect GetOwnerBounds()
        {
            if (!ownerIsControl)
                return GetOwnerWindowBounds();

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

        Rect GetOwnerWindowBounds() =>
            new(
                ownerWindow.Left,
                ownerWindow.Top,
                ownerWindow.ActualWidth,
                ownerWindow.ActualHeight
            );

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

    private enum Edge
    {
        None = -1,
        Left = 0,
        Right,
        Top,
        Bottom
    }
}
