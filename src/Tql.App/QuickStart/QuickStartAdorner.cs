namespace Tql.App.QuickStart;

internal class QuickStartAdorner : Adorner
{
    public const int Distance = 6;

    private readonly Border _border;

    protected override int VisualChildrenCount => 1;

    public QuickStartAdorner(UIElement adornedElement)
        : base(adornedElement)
    {
        _border = new Border
        {
            BorderBrush = new SolidColorBrush(Color.FromRgb(87, 157, 255)),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(4),
            Margin = new Thickness(-Distance)
        };

        AddVisualChild(_border);
    }

    protected override Size MeasureOverride(Size constraint)
    {
        _border.Measure(constraint);

        return base.MeasureOverride(constraint);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _border.Arrange(new Rect(finalSize));

        return base.ArrangeOverride(finalSize);
    }

    protected override Visual GetVisualChild(int index)
    {
        return _border;
    }
}
