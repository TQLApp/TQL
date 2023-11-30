using System.Windows.Controls.Primitives;
using Tql.App.Themes.Attached;

namespace Tql.App.Support;

internal static class PopoverToolTipUtils
{
    public static readonly DependencyProperty PopoverToolTipProperty =
        DependencyProperty.RegisterAttached(
            "PopoverToolTip",
            typeof(PopoverToolTip),
            typeof(PopoverToolTipUtils),
            new PropertyMetadata(OnPopoverToolTipChanged)
        );

    public static PopoverToolTip GetPopoverToolTip(DependencyObject d)
    {
        return (PopoverToolTip)d.GetValue(PopoverToolTipProperty);
    }

    public static void SetPopoverToolTip(DependencyObject d, PopoverToolTip? value)
    {
        d.SetValue(PopoverToolTipProperty, value);
    }

    private static void OnPopoverToolTipChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e
    )
    {
        var value = (PopoverToolTip?)e.NewValue;

        var header = value?.Header;
        var content = value?.Content;

        var self = (FrameworkElement)d;

        if (string.IsNullOrEmpty(content))
        {
            self.ToolTip = null;
            return;
        }

        // Unsure why this issue occurs, but sometimes the style can't be resolved.
        if (self.FindResource("PopoverToolTip") is not Style popoverToolTipStyle)
            return;

        var toolTip = new ToolTip
        {
            FontSize = SystemFonts.MessageFontSize,
            Content = content,
            Style = popoverToolTipStyle,
            CustomPopupPlacementCallback = PlacePopup
        };

        self.ToolTip = toolTip;

        ToolTipHeaderHelper.SetValue(toolTip, header ?? string.Empty);
        ToolTipHeaderHelper.SetVisibility(
            toolTip,
            string.IsNullOrEmpty(header) ? Visibility.Collapsed : Visibility.Visible
        );
        ToolTipService.SetInitialShowDelay(self, 0);
        ToolTipService.SetPlacement(self, PlacementMode.Custom);
        ToolTipService.SetBetweenShowDelay(self, 0);
        ToolTipService.SetShowDuration(self, int.MaxValue);
    }

    private static CustomPopupPlacement[] PlacePopup(Size popupsize, Size targetsize, Point offset)
    {
        return new[]
        {
            new CustomPopupPlacement(
                new Point(-(popupsize.Width - targetsize.Width) / 2, targetsize.Height + 10),
                PopupPrimaryAxis.Horizontal
            )
        };
    }
}

internal record PopoverToolTip(string? Header, string? Content);
