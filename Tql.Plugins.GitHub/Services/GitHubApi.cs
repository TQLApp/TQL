using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using System.Windows.Interop;
using Microsoft.Extensions.Logging;
using Octokit;
using Tql.Abstractions;
using Tql.Plugins.GitHub.ConfigurationUI;
using Tql.Utilities;
using IWin32Window = System.Windows.Forms.IWin32Window;
using ProductHeaderValue = Octokit.ProductHeaderValue;

namespace Tql.Plugins.GitHub.Services;

internal class GitHubApi
{
    private const string ClientId = "b5cf8dfb10c01dcfd22f";
    private const string RedirectUrl = "http://127.0.0.1:23119/complete";
    private const string Scope = "repo,project";

    private readonly ILogger<GitHubApi> _logger;
    private readonly IUI _ui;
    private readonly ConfigurationManager _configurationManager;
    private readonly IStore _store;
    private readonly HttpClient _httpClient;
    private readonly IEncryption _encryption;
    private readonly AsyncLock _lock = new();
    private readonly Dictionary<Guid, GitHubClient> _clients = new();

    public GitHubApi(
        ILogger<GitHubApi> logger,
        IUI ui,
        ConfigurationManager configurationManager,
        IStore store,
        HttpClient httpClient,
        IEncryption encryption
    )
    {
        _logger = logger;
        _ui = ui;
        _configurationManager = configurationManager;
        _store = store;
        _httpClient = httpClient;
        _encryption = encryption;
    }

    public async Task<GitHubClient> GetClient(Guid id)
    {
        using (await _lock.LockAsync())
        {
            if (!_clients.TryGetValue(id, out var client))
            {
                var connection = _configurationManager.Configuration.Connections.Single(
                    p => p.Id == id
                );

                try
                {
                    client = await CreateClient(connection);
                }
                catch
                {
                    _ui.ShowNotificationBar(
                        $"{GitHubPlugin.Id}/ConnectionFailed/{id}",
                        string.Format(
                            Labels.GitHubApi_UnableToConnect,
                            string.Format(Labels.GitHubApi_ResourceName, connection.Name)
                        ),
                        () => RetryConnect(id)
                    );
                    throw;
                }

                _clients[id] = client;
            }

            return client;
        }
    }

    private async void RetryConnect(Guid id)
    {
        try
        {
            await GetClient(id);
        }
        catch
        {
            // Ignore.
        }
    }

    private async Task<GitHubClient> CreateClient(Connection connection)
    {
        var appVersion = GetType().Assembly.GetName().Version;

        var client = new GitHubClient(new ProductHeaderValue("TQL", appVersion.ToString()));

        // PAT tokens are only used by unit tests.

        if (connection.PatToken != null)
        {
            client.Credentials = new Credentials(connection.PatToken, AuthenticationType.Bearer);
            return client;
        }

        var credentials = ReadCredentials(connection.Id);

        // Only use the token if the scope is the same.

        if (credentials is { Scope: Scope })
        {
            client.Credentials = new Credentials(
                credentials.AccessToken,
                AuthenticationType.Bearer
            );

            try
            {
                await client.User.Current();
            }
            catch
            {
                client.Credentials = Credentials.Anonymous;
            }
        }

        if (client.Credentials.AuthenticationType != AuthenticationType.Bearer)
        {
            await _ui.PerformInteractiveAuthentication(
                new InteractiveAuthentication(
                    string.Format(Labels.GitHubApi_ResourceName, connection.Name),
                    client,
                    _httpClient,
                    _ui
                )
            );

            if (client.Credentials.AuthenticationType != AuthenticationType.Bearer)
                throw new GitHubAuthenticationException("Authentication failed");

            WriteCredentials(connection.Id, new CredentialsDto(client.Credentials.Password, Scope));
        }

        return client;
    }

    private CredentialsDto? ReadCredentials(Guid id)
    {
        try
        {
            var connection = _configurationManager.Configuration.Connections.Single(
                p => p.Id == id
            );

            var credentials = _encryption.DecryptString(connection.ProtectedCredentials);
            if (credentials == null)
                return null;

            return JsonSerializer.Deserialize<CredentialsDto>(credentials);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load credentials for connection {ConnectionId}", id);
        }

        return null;
    }

    private void WriteCredentials(Guid id, CredentialsDto credentialsDto)
    {
        var json = JsonSerializer.Serialize(credentialsDto);

        var protectedCredentials = _encryption.EncryptString(json)!;

        _configurationManager.UpdateCredentials(id, protectedCredentials);
    }

    private class InteractiveAuthentication : IInteractiveAuthentication
    {
        private readonly GitHubClient _client;
        private readonly HttpClient _httpClient;
        private readonly IUI _ui;

        public string ResourceName { get; }

        public InteractiveAuthentication(
            string resourceName,
            GitHubClient client,
            HttpClient httpClient,
            IUI ui
        )
        {
            _client = client;
            _httpClient = httpClient;
            _ui = ui;
            ResourceName = resourceName;
        }

        public async Task Authenticate(IWin32Window owner)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://github.com/login/device/code?client_id={Uri.EscapeDataString(ClientId)}&scope={Uri.EscapeDataString(Scope)}"
            );

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();

            var dto = JsonSerializer.Deserialize<DeviceCodeLoginDto>(stream)!;

            var window = new DeviceCodeWindow(dto, ClientId, _ui, _httpClient)
            {
                Owner = HwndSource.FromHwnd(owner.Handle)?.RootVisual as Window
            };

            window.ShowDialog();

            window.Exception?.Throw();

            if (window.AccessToken == null)
                throw new GitHubAuthenticationException("Unexpected error");

            _client.Credentials = new Credentials(window.AccessToken, AuthenticationType.Bearer);

            await _client.User.Current();
        }
    }

    private record CredentialsDto(string AccessToken, string Scope);
}

internal record DeviceCodeLoginDto(
    [property: JsonPropertyName("device_code")] string DeviceCode,
    [property: JsonPropertyName("user_code")] string UserCode,
    [property: JsonPropertyName("verification_uri")] string VerificationUri,
    [property: JsonPropertyName("expires_in")] int ExpiresIn,
    [property: JsonPropertyName("interval")] int Interval
);
