using System.Windows.Interop;
using Tql.Abstractions;
using Tql.App.Support;
using Tql.Utilities;

namespace Tql.App.Services;

internal partial class InteractiveAuthenticationWindow
{
    private readonly IInteractiveAuthentication _interactiveAuthentication;
    private readonly IUI _ui;

    public Exception? Exception { get; private set; }

    public InteractiveAuthenticationWindow(
        IInteractiveAuthentication interactiveAuthentication,
        IUI ui
    )
    {
        _interactiveAuthentication = interactiveAuthentication;
        _ui = ui;

        InitializeComponent();

        _plugin.Text = interactiveAuthentication.ResourceName;
    }

    private async void _acceptButton_Click(object sender, RoutedEventArgs e)
    {
        var handle = new WindowInteropHelper(this).Handle;

        while (true)
        {
            try
            {
                await _interactiveAuthentication.Authenticate(new Win32Window(handle));

                Close();
                return;
            }
            catch (Exception ex)
            {
                var result = _ui.ShowException(
                    this,
                    Labels.InteractiveAuthenticationWindow_FailedToAuthenticate,
                    ex,
                    buttons: DialogCommonButtons.Retry | DialogCommonButtons.Cancel
                );

                if (result != System.Windows.Forms.DialogResult.Retry)
                {
                    Exception = ex;

                    Close();
                    return;
                }
            }
        }
    }
}
