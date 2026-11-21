using Tql.Abstractions;
using Tql.Plugins.YouTrack.Services;
using Tql.Utilities;
using WinFormsDialogResult = System.Windows.Forms.DialogResult;

namespace Tql.Plugins.YouTrack.ConfigurationUI;

internal partial class ConnectionEditWindow
{
    private readonly IUI _ui;
    private readonly YouTrackApi _api;
    private readonly CancellationTokenSource _closedCts = new();

    public ConnectionEditWindow(IUI ui, YouTrackApi api)
    {
        _ui = ui;
        _api = api;

        InitializeComponent();

        Closed += (_, _) => _closedCts.Cancel();
    }

    private async void _acceptButton_Click(object sender, RoutedEventArgs e)
    {
        var dto = (ConnectionDto)DataContext;
        var cancellationToken = _closedCts.Token;

        IsEnabled = false;

        try
        {
            var serviceUrl = await CheckConnection(dto.Url!, dto.Token!, cancellationToken);
            if (serviceUrl == null || cancellationToken.IsCancellationRequested)
                return;

            dto.Url = serviceUrl;

            DialogResult = true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // The window was closed while the connection was being checked.
        }
        finally
        {
            IsEnabled = true;
        }
    }

    // Checks the connection in two steps: YouTrack's REST API must answer at
    // the URL, or at one of its parent paths, which the dialog then offers;
    // and YouTrack must accept the token. Returns the service URL, or null
    // when the connection can't be saved as it is.
    private async Task<string?> CheckConnection(
        string url,
        string token,
        CancellationToken cancellationToken
    )
    {
        YouTrackServiceCheck check;

        try
        {
            check = await _api.CheckServiceUrl(url, cancellationToken);
        }
        catch (Exception ex) when (ex is YouTrackNotAnApiException or ArgumentException)
        {
            _ui.ShowAlert(
                this,
                string.Format(Labels.ConnectionEditWindow_NotYouTrack, url),
                Labels.ConnectionEditWindow_NotYouTrackHelpText
            );
            return null;
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            _ui.ShowException(
                this,
                string.Format(Labels.ConnectionEditWindow_UnableToConnect, url),
                ex
            );
            return null;
        }

        // A URL copied from the address bar, such as
        // https://youtrack.example.com/issues, answers with the web app rather
        // than the REST API, which answered on a parent path instead.
        if (check.IsSuggestion)
        {
            var result = _ui.ShowConfirmation(
                this,
                string.Format(Labels.ConnectionEditWindow_DidYouMean, check.ServiceUrl),
                string.Format(Labels.ConnectionEditWindow_DidYouMeanHelpText, check.EnteredUrl),
                DialogCommonButtons.Yes | DialogCommonButtons.No,
                DialogIcon.Information
            );

            if (result != WinFormsDialogResult.Yes)
                return null;
        }

        try
        {
            await _api.GetClient(check.ServiceUrl, token).GetCurrentUser(cancellationToken);
        }
        catch (YouTrackUnauthorizedException)
        {
            _ui.ShowAlert(
                this,
                Labels.ConnectionEditWindow_TokenRefused,
                string.Format(Labels.ConnectionEditWindow_TokenRefusedHelpText, check.ServiceUrl)
            );
            return null;
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            _ui.ShowException(
                this,
                string.Format(Labels.ConnectionEditWindow_UnableToConnect, check.ServiceUrl),
                ex
            );
            return null;
        }

        return check.ServiceUrl;
    }

    private void _documentation_Click(object sender, RoutedEventArgs e)
    {
        _ui.OpenUrl("https://github.com/TQLApp/TQL/wiki/YouTrack-plugin");
    }
}
