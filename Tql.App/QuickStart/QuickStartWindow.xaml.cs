using System.Windows.Interop;
using Tql.Abstractions;
using Tql.App.Support;

namespace Tql.App.QuickStart;

internal partial class QuickStartWindow
{
    private readonly IUI _ui;
    private static readonly QuickStartMarkdownRenderer MarkdownRenderer = new();

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text),
        typeof(string),
        typeof(QuickStartWindow),
        new PropertyMetadata(
            string.Empty,
            (d, e) => ((QuickStartWindow)d).ApplyText((string)e.NewValue)
        )
    );

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public event EventHandler<QuickStartPopupButtonEventArgs>? ChoiceButtonClicked;
    public event EventHandler<QuickStartPopupButtonEventArgs>? ButtonClicked;
    public event EventHandler? DismissClicked;

    public Visibility ArrowVisibility
    {
        get => _arrow.Visibility;
        set => _arrow.Visibility = value;
    }

    public double ContentWidth
    {
        get => _content.Width;
        set => _content.Width = value;
    }

    public QuickStartWindow(IUI ui)
    {
        _ui = ui;

        InitializeComponent();

        _dismiss.Source = Images.QuickStartDismiss;
        _dismiss.AttachOnClickHandler(OnDismissClicked);
    }

    private void ChoiceButton_Click(object? sender, RoutedEventArgs e)
    {
        var button = (Button)sender!;

        OnChoiceButtonClicked(
            new QuickStartPopupButtonEventArgs((QuickStartPopupButton)button.DataContext)
        );
    }

    private void Button_Click(object? sender, RoutedEventArgs e)
    {
        var button = (Button)sender!;

        OnButtonClicked(
            new QuickStartPopupButtonEventArgs((QuickStartPopupButton)button.DataContext)
        );
    }

    private void ApplyText(string text)
    {
        _lines.Items.Clear();

        foreach (var textBlock in MarkdownRenderer.Render(this, text, _ui))
        {
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.Margin = new Thickness(5);

            _lines.Items.Add(textBlock);
        }
    }

    protected virtual void OnChoiceButtonClicked(QuickStartPopupButtonEventArgs e) =>
        ChoiceButtonClicked?.Invoke(this, e);

    protected virtual void OnButtonClicked(QuickStartPopupButtonEventArgs e) =>
        ButtonClicked?.Invoke(this, e);

    protected virtual void OnDismissClicked() => DismissClicked?.Invoke(this, EventArgs.Empty);

    private void BaseWindow_SourceInitialized(object? sender, EventArgs e)
    {
        WindowInterop.HideFromTaskSwitcher(new WindowInteropHelper(this).Handle);
    }
}
