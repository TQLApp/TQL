namespace Tql.App.Support;

internal static class FrameworkElementExtensions
{
    public static void SetPopoverToolTip(this FrameworkElement self, string? content)
    {
        self.SetPopoverToolTip(null, content);
    }

    public static void SetPopoverToolTip(
        this FrameworkElement self,
        string? header,
        string? content
    )
    {
        PopoverToolTipUtils.SetPopoverToolTip(self, new PopoverToolTip(header, content));
    }
}
