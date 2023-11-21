namespace Tql.App.QuickStart;

internal class QuickStartPopupButtonEventArgs(QuickStartPopupButton button) : EventArgs
{
    public QuickStartPopupButton Button { get; } = button;
}
