using System.Windows.Forms;
using Tql.Abstractions;
using Tql.App.Services;

namespace Tql.App.QuickStart;

internal class QuickStartManager
{
    private readonly Settings _settings;
    private readonly IUI _ui;
    private QuickStartDto _state;

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
        _ui = ui;
        _state = QuickStartDto.FromSettings(settings);
    }

    public void Show(
        FrameworkElement owner,
        QuickStartPopup popup,
        QuickStartPopupMode mode = QuickStartPopupMode.None
    )
    {
        var ownerWindow =
            Window.GetWindow(owner) ?? throw new InvalidOperationException("Cannot resolve window");

        var window = new QuickStartWindow { DataContext = popup };

        window.ChoiceButtonClicked += (_, e) =>
        {
            window.Close();
            e.Button.Action();
        };

        window.ButtonClicked += (_, e) =>
        {
            window.Close();
            e.Button.Action();
        };

        window.DismissClicked += (_, _) =>
        {
            var result = _ui.ShowConfirmation(
                window,
                "Are you sure?",
                "Are you sure you want to close the Quick Start guide? You can restart the Quick Start guide from the settings screen at any time."
            );

            if (result == DialogResult.Yes)
            {
                window.Close();
                State = State with { IsDismissed = true };
            }
        };

        window.Owner = ownerWindow;

        window.SourceInitialized += (_, _) =>
        {
            // Get all bounds and sizes, and the screen we're working with,
            // adjusted for DPI scaling.
            var source = PresentationSource.FromVisual(ownerWindow)!;

            var scaleX = source.CompositionTarget!.TransformToDevice.M11;
            var scaleY = source.CompositionTarget!.TransformToDevice.M22;

            var ownerWindowBounds = ScaleBounds(
                new Rect(
                    ownerWindow.Left,
                    ownerWindow.Top,
                    ownerWindow.ActualWidth,
                    ownerWindow.ActualHeight
                )
            );
            var windowSize = ScaleSize(new Size(window.ActualWidth, window.ActualHeight));
            var showOnScreen = ShowOnScreenManager.Create(_settings.ShowOnScreen);
            var screen = showOnScreen.GetScreen();

            // We base all random values on the hash code of the popup. This
            // means that the location of the popup will be stable, for a specific
            // popup.
            var random = new Random(popup.GetHashCode());

            // Calculate the desired offset off of the center of the owner window.
            var xRange = Math.Max(ownerWindowBounds.Width - (windowSize.Width / 2), 0);
            var yRange = Math.Max(ownerWindowBounds.Height - (windowSize.Height / 2), 0);
            var xOffset = random.Next(-(int)(xRange / 2), (int)(xRange / 2));
            var yOffset = random.Next(-(int)(yRange / 2), (int)(yRange / 2));

            // Adjust the offsets for the target window size.
            xOffset -= (int)windowSize.Width / 2;
            yOffset -= (int)windowSize.Height / 2;

            // Calculate the center of the owner window as the starting point.
            var x = ownerWindowBounds.Left + ownerWindowBounds.Width / 2;
            var y = ownerWindowBounds.Top + ownerWindowBounds.Height / 2;

            // Ensure the quick start window is always this amount of pixels from
            // the owner window and the screen border.
            var edgeDistance = 10 * scaleX;

            if (mode.HasFlag(QuickStartPopupMode.Modal))
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
                        y = ownerWindowBounds.Top - (windowSize.Height + edgeDistance);
                        break;

                    case 1: // East
                        x = ownerWindowBounds.Right + edgeDistance;
                        y += yOffset;
                        break;

                    case 2: // South
                        x += xOffset;
                        y = ownerWindowBounds.Bottom + edgeDistance;
                        break;

                    case 3: // West
                        x = ownerWindowBounds.Left - (windowSize.Width + edgeDistance);
                        y += yOffset;
                        break;

                    default:
                        throw new InvalidOperationException();
                }
            }

            // Make sure the quick start window is visible on the screen.
            if (x < screen.Bounds.Left + edgeDistance)
                x = screen.Bounds.Left + edgeDistance;
            else if (x + windowSize.Width > screen.Bounds.Right - edgeDistance)
                x = screen.Bounds.Right - edgeDistance - windowSize.Width;
            if (y < screen.Bounds.Top + edgeDistance)
                y = screen.Bounds.Top + edgeDistance;
            else if (y + windowSize.Height > screen.Bounds.Bottom - edgeDistance)
                y = screen.Bounds.Bottom - edgeDistance - windowSize.Height;

            window.Left = x / scaleX;
            window.Top = y / scaleY;

            Rect ScaleBounds(Rect bounds) =>
                new(
                    bounds.Left * scaleX,
                    bounds.Top * scaleY,
                    bounds.Width * scaleX,
                    bounds.Height * scaleY
                );

            Size ScaleSize(Size size) => new(size.Width * scaleX, size.Height * scaleY);
        };

        if (mode.HasFlag(QuickStartPopupMode.Modal))
            window.ShowDialog();
        else
            window.Show();
    }
}
