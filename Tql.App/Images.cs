using Tql.Utilities;

namespace Tql.App;

internal static class Images
{
    private static DrawingImage LoadImage(string resourceName)
    {
        using var stream = Application
            .GetResourceStream(new Uri($"/Resources/{resourceName}", UriKind.Relative))!
            .Stream;

        return ImageFactory.CreateSvgImage(stream!);
    }

    public static readonly DrawingImage Run = LoadImage("Person Running.svg");
    public static readonly DrawingImage Star = LoadImage("Star.svg");
    public static readonly DrawingImage Dismiss = LoadImage("Dismiss.svg");
    public static readonly DrawingImage Category = LoadImage("Apps List.svg");
    public static readonly DrawingImage Copy = LoadImage("Copy.svg");
    public static readonly DrawingImage Backspace = LoadImage("Backspace.svg");
    public static readonly DrawingImage Help = LoadImage("Help.svg");
    public static readonly DrawingImage Settings = LoadImage("Settings.svg");
    public static readonly DrawingImage CommentNote = LoadImage("Comment Note.svg");
    public static readonly DrawingImage Astronaut = LoadImage("Astronaut.svg");
    public static readonly DrawingImage Undo = LoadImage("Undo.svg");
    public static readonly DrawingImage Pin = LoadImage("Pin.svg");
    public static readonly DrawingImage PinOff = LoadImage("Pin Off.svg");
}
