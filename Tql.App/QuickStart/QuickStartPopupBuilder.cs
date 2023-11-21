namespace Tql.App.QuickStart;

internal class QuickStartPopupBuilder(IPlaybook playbook, string id, object[] args)
{
    private readonly ImmutableArray<QuickStartPopupButton>.Builder _choiceButtons =
        ImmutableArray.CreateBuilder<QuickStartPopupButton>();
    private readonly ImmutableArray<QuickStartPopupButton>.Builder _buttons =
        ImmutableArray.CreateBuilder<QuickStartPopupButton>();

    public QuickStartPopupBuilder WithChoiceButton(string label, Action action)
    {
        _choiceButtons.Add(new QuickStartPopupButton(label, action));
        return this;
    }

    public QuickStartPopupBuilder WithButton(string label, Action action)
    {
        _buttons.Add(new QuickStartPopupButton(label, action));
        return this;
    }

    public QuickStartPopup Build()
    {
        return new QuickStartPopup(
            playbook,
            id,
            args,
            _choiceButtons.ToImmutable(),
            _buttons.ToImmutable()
        );
    }
}
