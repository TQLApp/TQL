using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Tql.App.QuickStart;

internal class QuickStartPopup : INotifyPropertyChanged
{
    private readonly object[] _args;

    public static QuickStartPopupBuilder CreateBuilder(
        IPlaybook playbook,
        string id,
        params object[] args
    ) => new(playbook, id, args);

    private PropertyChangedEventHandler? _propertyChanged;

    public IPlaybook Playbook { get; }
    public string Id { get; }
    public ImmutableArray<QuickStartPopupButton> ChoiceButtons { get; }
    public ImmutableArray<QuickStartPopupButton> Buttons { get; }
    public Action? Back { get; }

    public string Title { get; private set; } = null!;
    public string Text { get; private set; } = null!;

    public event PropertyChangedEventHandler? PropertyChanged
    {
        add
        {
            if (_propertyChanged == null)
                Playbook.Updated += Playbook_Updated;
            _propertyChanged += value;
        }
        remove
        {
            _propertyChanged -= value;
            if (_propertyChanged == null)
                Playbook.Updated -= Playbook_Updated;
        }
    }

    public QuickStartPopup(
        IPlaybook playbook,
        string id,
        object[] args,
        ImmutableArray<QuickStartPopupButton> choiceButtons,
        ImmutableArray<QuickStartPopupButton> buttons,
        Action? back
    )
    {
        _args = args;
        Playbook = playbook;
        Id = id;
        ChoiceButtons = choiceButtons;
        Buttons = buttons;
        Back = back;

        ReloadPage();
    }

    private void ReloadPage()
    {
        var page = Playbook[Id];

        Title = page.Title;
        Text = page.Text;

        if (_args.Length > 0)
        {
            Title = string.Format(Title, _args);
            Text = string.Format(Text, _args);
        }

        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Text));
    }

    private void Playbook_Updated(object? sender, EventArgs e) => ReloadPage();

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

internal record QuickStartPopupButton(string Label, Action Action);
