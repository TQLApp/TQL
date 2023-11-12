namespace Tql.App.QuickStart;

internal class QuickStartPopupBuilder
{
    private readonly IPlaybook _playbook;
    private readonly string _id;
    private readonly object[] _args;

    private readonly ImmutableArray<QuickStartPopupButton>.Builder _choiceButtons =
        ImmutableArray.CreateBuilder<QuickStartPopupButton>();
    private readonly ImmutableArray<QuickStartPopupButton>.Builder _buttons =
        ImmutableArray.CreateBuilder<QuickStartPopupButton>();

    public QuickStartPopupBuilder(IPlaybook playbook, string id, object[] args)
    {
        _playbook = playbook;
        _id = id;
        _args = args;
    }

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
            _playbook,
            _id,
            _args,
            _choiceButtons.ToImmutable(),
            _buttons.ToImmutable()
        );
    }
}
