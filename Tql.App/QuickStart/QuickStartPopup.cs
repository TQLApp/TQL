namespace Tql.App.QuickStart;

internal record QuickStartPopup(
    string Title,
    ImmutableArray<string> Lines,
    ImmutableArray<QuickStartPopupButton> ChoiceButtons,
    ImmutableArray<QuickStartPopupButton> Buttons
)
{
    public static QuickStartPopupBuilder CreateBuilder() => new();
}

internal record QuickStartPopupButton(string Label, Action Action);
