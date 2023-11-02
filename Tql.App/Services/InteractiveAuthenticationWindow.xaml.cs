using System.Windows.Interop;
using Tql.Abstractions;
using Tql.App.Support;
using Tql.Interop;

namespace Tql.App.Services;

internal partial class InteractiveAuthenticationWindow
{
    private readonly IInteractiveAuthentication _interactiveAuthentication;

    public Exception? Exception { get; private set; }

    public InteractiveAuthenticationWindow(IInteractiveAuthentication interactiveAuthentication)
    {
        _interactiveAuthentication = interactiveAuthentication;

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
                var result = TaskDialogEx.Error(
                    this,
                    Labels.InteractiveAuthenticationWindow_FailedToAuthenticate,
                    ex,
                    buttons: TaskDialogCommonButtons.Retry | TaskDialogCommonButtons.Cancel
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
