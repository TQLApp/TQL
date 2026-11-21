using System.Net;
using System.Net.Http;
using Tql.Abstractions;

namespace Tql.Plugins.YouTrack.Services;

internal class YouTrackApi(HttpClient httpClient, IUI ui, IEncryption encryption)
{
    /// <summary>
    /// A client for a configured connection. A 401 shows the "unable to
    /// connect" notification bar, any other answer removes it.
    /// </summary>
    public YouTrackClient GetClient(Connection connection)
    {
        return new YouTrackClient(
            httpClient,
            connection.Url,
            encryption.DecryptString(connection.ProtectedToken) ?? "",
            p => ShowUnauthorizedNotification(connection, p)
        );
    }

    /// <summary>
    /// A client for a connection that's being edited, for the connection check.
    /// </summary>
    public YouTrackClient GetClient(string serviceUrl, string token)
    {
        return new YouTrackClient(httpClient, serviceUrl, token);
    }

    public Task<YouTrackServiceCheck> CheckServiceUrl(
        string url,
        CancellationToken cancellationToken = default
    )
    {
        return YouTrackClient.CheckServiceUrl(httpClient, url, cancellationToken);
    }

    private void ShowUnauthorizedNotification(Connection connection, HttpStatusCode statusCode)
    {
        var notificationKey = $"{YouTrackPlugin.Id}/ConnectionFailed/{connection.Id}";

        if (statusCode == HttpStatusCode.Unauthorized)
        {
            ui.ShowNotificationBar(
                notificationKey,
                string.Format(
                    Labels.YouTrackApi_UnableToConnect,
                    string.Format(Labels.YouTrackApi_ResourceName, connection.Name),
                    Labels.YouTrackPlugin_Title
                ),
                _ => ui.OpenConfiguration(YouTrackPlugin.ConfigurationPageId)
            );
        }
        else
        {
            ui.RemoveNotificationBar(notificationKey);
        }
    }
}
