using System.Windows.Controls.Primitives;
using Tql.App.Themes.Attached;

namespace Tql.App.Support;

internal static class TooltipUtil
{
    public static void SetPopoverToolTip(this FrameworkElement self, string? content)
    {
        SetPopoverToolTip(self, null, content);
    }

    public static void SetPopoverToolTip(
        this FrameworkElement self,
        string? header,
        string? content
    )
    {
        if (string.IsNullOrEmpty(content))
            return;

        var toolTip = new ToolTip
        {
            FontSize = SystemFonts.MessageFontSize,
            Content = content,
            Style = (Style)self.FindResource("PopoverToolTip"),
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
