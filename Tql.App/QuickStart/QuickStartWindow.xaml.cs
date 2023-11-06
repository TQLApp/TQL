using System.Runtime.InteropServices;
using System.Windows.Interop;
using Tql.App.Interop;
using Tql.App.Support;

namespace Tql.App.QuickStart;

internal partial class QuickStartWindow
{
    public event EventHandler<QuickStartPopupButtonEventArgs>? ChoiceButtonClicked;
    public event EventHandler<QuickStartPopupButtonEventArgs>? ButtonClicked;
    public event EventHandler? DismissClicked;

    public QuickStartWindow()
    {
        InitializeComponent();

        _dismiss.Source = Images.QuickStartDismiss;
        _dismiss.AttachOnClickHandler(OnDismissClicked);
    }

    private void ChoiceButton_Click(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;

        OnChoiceButtonClicked(
            new QuickStartPopupButtonEventArgs((QuickStartPopupButton)button.DataContext)
        );
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;

        OnButtonClicked(
            new QuickStartPopupButtonEventArgs((QuickStartPopupButton)button.DataContext)
        );
    }

    protected virtual void OnChoiceButtonClicked(QuickStartPopupButtonEventArgs e) =>
        ChoiceButtonClicked?.Invoke(this, e);

    protected virtual void OnButtonClicked(QuickStartPopupButtonEventArgs e) =>
        ButtonClicked?.Invoke(this, e);

    protected virtual void OnDismissClicked() => DismissClicked?.Invoke(this, EventArgs.Empty);
}
