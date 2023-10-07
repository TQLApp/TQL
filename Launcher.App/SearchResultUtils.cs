using Launcher.Abstractions;
using Launcher.App.Search;
using Launcher.App.Support;
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

    public static UIElement RenderMatch(
        IMatch match,
        TextMatch? textMatch,
        bool isFuzzyMatch,
        double? fontSize = null,
        bool wrapTextInMarquee = false
    )
    {
        var icon = new Image
        {
            Source = ((Services.Image)match.Icon).ImageSource,
            Width = 18,
            Height = 18,
            Margin = new Thickness(0, 0, 6, 0),
            VerticalAlignment = VerticalAlignment.Center
        };

        var textBlock = new TextBlock();

        if (fontSize.HasValue)
            textBlock.FontSize = fontSize.Value;

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
                    var inline = new Bold(new Run(part));

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

        var grid = new Grid();

        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
        );

        grid.Children.Add(icon);

        if (wrapTextInMarquee)
        {
            var marquee = new MarqueeControl { Content = textBlock };

            grid.Children.Add(marquee);

            Grid.SetColumn(marquee, 1);
        }
        else
        {
            grid.Children.Add(textBlock);

            Grid.SetColumn(textBlock, 1);
        }

        return grid;
    }
}
