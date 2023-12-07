namespace Tql.App.QuickStart;

internal class QuickStartPopupBuilder(IPlaybook playbook, string id, object[] args)
{
    private readonly ImmutableArray<QuickStartPopupButton>.Builder _choiceButtons =
        ImmutableArray.CreateBuilder<QuickStartPopupButton>();
    private readonly ImmutableArray<QuickStartPopupButton>.Builder _buttons =
        ImmutableArray.CreateBuilder<QuickStartPopupButton>();
    private Action? _back;

    public QuickStartPopupBuilder WithChoiceButton(string label, Action action)
    {
        _choiceButtons.Add(new QuickStartPopupButton(label, false, action));
        return this;
    }

    public QuickStartPopupBuilder WithButton(string label, Action action)
    {
        return WithButton(label, false, action);
    }

    public QuickStartPopupBuilder WithButton(string label, bool isDefault, Action action)
    {
        _buttons.Add(new QuickStartPopupButton(label, isDefault, action));
        return this;
    }

    public QuickStartPopupBuilder WithBack(Action? back)
    {
        _back = back;
        return this;
    }

    public QuickStartPopup Build()
    {
        return new QuickStartPopup(
            playbook,
            id,
            args,
            _choiceButtons.ToImmutable(),
            _buttons.ToImmutable(),
            _back
        );
    }
}
