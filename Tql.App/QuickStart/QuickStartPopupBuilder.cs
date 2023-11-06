using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Tql.App.QuickStart;

internal class QuickStartPopupBuilder
{
    private string? _title;
    private readonly ImmutableArray<string>.Builder _lines = ImmutableArray.CreateBuilder<string>();
    private readonly ImmutableArray<QuickStartPopupButton>.Builder _choiceButtons =
        ImmutableArray.CreateBuilder<QuickStartPopupButton>();
    private readonly ImmutableArray<QuickStartPopupButton>.Builder _buttons =
        ImmutableArray.CreateBuilder<QuickStartPopupButton>();

    public QuickStartPopupBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public QuickStartPopupBuilder WithText(string text)
    {
        foreach (var line in Regex.Split(text.TrimEnd(), @"(?:\r?\n){2}"))
        {
            WithLine(Regex.Replace(line, @"\r?\n", " "));
        }

        return this;
    }

    public QuickStartPopupBuilder WithLine(string line)
    {
        _lines.Add(line);
        return this;
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
        if (_title == null)
            throw new InvalidOperationException("Title is required");

        return new QuickStartPopup(
            _title,
            _lines.ToImmutable(),
            _choiceButtons.ToImmutable(),
            _buttons.ToImmutable()
        );
    }
}
