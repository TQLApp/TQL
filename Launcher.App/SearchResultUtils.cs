using Launcher.Abstractions;
using Launcher.App.Search;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;

namespace Launcher.App;

internal static class SearchResultUtils
{
    private static readonly TextDecoration WavyLineDecoration =
        new()
        {
            Location = TextDecorationLocation.Underline,
            Pen = new Pen(
                new VisualBrush
                {
                    Viewbox = new Rect(0, 0, 2, 2),
                    ViewboxUnits = BrushMappingMode.Absolute,
                    Viewport = new Rect(0, 2.3, 6, 4),
                    ViewportUnits = BrushMappingMode.Absolute,
                    TileMode = TileMode.Tile,
                    Visual = new Path
                    {
                        Data = Geometry.Parse("M 0,1 C 1,0 1,2 2,1"),
                        Stroke = Brushes.Red,
                        StrokeThickness = 0.2,
                        StrokeStartLineCap = PenLineCap.Square,
                        StrokeEndLineCap = PenLineCap.Square
                    }
                },
                6
            )
        };

    public static void RenderMatch(
        UIElementCollection collection,
        IMatch match,
        TextMatch? textMatch,
        bool isFuzzyMatch,
        double? fontSize = null
    )
    {
        collection.Add(
            new Image
            {
                Source = ((Services.Image)match.Icon).ImageSource,
                Width = 18,
                Height = 18,
                Margin = new Thickness(0, 0, 6, 0),
                VerticalAlignment = VerticalAlignment.Center
            }
        );

        var textBlock = new TextBlock();

        if (fontSize.HasValue)
            textBlock.FontSize = fontSize.Value;

        collection.Add(textBlock);

        var offset = 0;

        var text = match.Text;

        if (textMatch != null)
        {
            foreach (var range in textMatch.Ranges)
            {
                if (offset < range.Offset)
                {
                    var part = text.Substring(offset, range.Offset - offset);
                    textBlock.Inlines.Add(new Run(part));
                }

                if (range.Length > 0)
                {
                    var part = text.Substring(range.Offset, range.Length);

                    // HACK: No idea why the bold run renders as Black when the
                    // explicit foreground color is removed here.
                    var inline = new Bold(new Run(part)) { Foreground = Brushes.White };

                    if (isFuzzyMatch)
                        inline.TextDecorations.Add(WavyLineDecoration);

                    textBlock.Inlines.Add(inline);
                }

                offset = range.Offset + range.Length;
            }
        }

        if (offset < text.Length)
        {
            var part = text.Substring(offset);
            textBlock.Inlines.Add(new Run(part));
        }
    }
}
