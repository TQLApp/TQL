namespace Tql.App.QuickStart;

internal class QuickStartPopupButtonEventArgs : EventArgs
{
    public QuickStartPopupButton Button { get; }

    public QuickStartPopupButtonEventArgs(QuickStartPopupButton button)
    {
        Button = button;
    }
}
