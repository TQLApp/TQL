using System.Collections.Concurrent;
using Tql.Utilities;

namespace Tql.App;

internal static class Images
{
    private static readonly ConcurrentDictionary<
        (string ResourceName, Color? Color),
        DrawingImage
    > LoadedImages = new();

    public static DrawingImage GetImage(string resourceName, Color? color = null)
    {
        return LoadedImages.GetOrAdd(
            (resourceName, color),
            p => LoadImage(p.ResourceName, p.Color)
        );
    }

    private static DrawingImage LoadImage(string resourceName, Color? color)
    {
        using var stream = Application
            .GetResourceStream(
                new Uri(
                    $"pack://application:,,,/Tql.App;component/Resources/{resourceName.Replace("+", " ")}"
                )
            )!
            .Stream;

        var fill = default(Brush);
        if (color.HasValue)
            fill = new SolidColorBrush(color.Value);

        return ImageFactory.CreateSvgImage(stream!, fill, null);
    }

    public static readonly DrawingImage Run = GetImage("Person Running.svg");
    public static readonly DrawingImage Star = GetImage("Star.svg");
    public static readonly DrawingImage Dismiss = GetImage("Dismiss.svg");
    public static readonly DrawingImage QuickStartDismiss = GetImage("Quick Start Dismiss.svg");
    public static readonly DrawingImage Category = GetImage("Apps List.svg");
    public static readonly DrawingImage Copy = GetImage("Copy.svg");
    public static readonly DrawingImage Backspace = GetImage("Backspace.svg");
    public static readonly DrawingImage Help = GetImage("Help.svg");
    public static readonly DrawingImage Settings = GetImage("Settings.svg");
    public static readonly DrawingImage CommentNote = GetImage("Comment Note.svg");
    public static readonly DrawingImage Astronaut = GetImage("Astronaut.svg");
    public static readonly DrawingImage UndoDark = GetImage(
        "Undo.svg",
        Color.FromRgb(240, 240, 240)
    );
    public static readonly DrawingImage UndoLight = GetImage("Undo.svg", Color.FromRgb(15, 15, 15));
    public static readonly DrawingImage Pin = GetImage("Pin.svg");
    public static readonly DrawingImage PinOff = GetImage("Pin Off.svg");
    public static readonly DrawingImage NuGet = GetImage("NuGet.svg");
    public static readonly DrawingImage CheckmarkCircle = GetImage("Checkmark Circle.svg");
    public static readonly DrawingImage Grab = GetImage("Grab.svg");
    public static readonly DrawingImage Verified = GetImage("Verified.svg");
}
