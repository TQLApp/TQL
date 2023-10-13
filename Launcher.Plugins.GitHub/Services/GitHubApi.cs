using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using System.Windows.Interop;
using Launcher.Abstractions;
using Launcher.Plugins.GitHub.ConfigurationUI;
using Microsoft.Extensions.Logging;
using NeoSmart.AsyncLock;
using Octokit;
using IWin32Window = System.Windows.Forms.IWin32Window;
using ProductHeaderValue = Octokit.ProductHeaderValue;

namespace Launcher.Plugins.GitHub.Services;

internal class GitHubApi
{
    private const string ClientId = "b5cf8dfb10c01dcfd22f";
    private const string RedirectUrl = "http://127.0.0.1:23119/complete";
    private const string Scope = "repo,project";

    private readonly ILogger<GitHubApi> _logger;
    private readonly IUI _ui;
    private readonly ConnectionManager _connectionManager;
    private readonly IStore _store;
    private readonly HttpClient _httpClient;
    private readonly AsyncLock _lock = new();
    private readonly Dictionary<Guid, GitHubClient> _clients = new();

    public GitHubApi(
        ILogger<GitHubApi> logger,
        IUI ui,
        ConnectionManager connectionManager,
        IStore store,
        HttpClient httpClient
    )
    {
        _logger = logger;
        _ui = ui;
        _connectionManager = connectionManager;
        _store = store;
        _httpClient = httpClient;
    }

    public async Task<GitHubClient> GetClient(Guid id)
    {
        using (await _lock.LockAsync())
        {
            if (!_clients.TryGetValue(id, out var client))
            {
                client = new GitHubClient(new ProductHeaderValue("Launcher"));

                var connection = _connectionManager.Connections.Single(p => p.Id == id);

                var credentials = ReadCredentials(id);

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
                            $"GitHub - {connection.Name}",
                            client,
                            _httpClient,
                            _ui
                        )
                    );

                    if (client.Credentials.AuthenticationType != AuthenticationType.Bearer)
                        throw new GitHubAuthenticationException("Authentication failed");

                    WriteCredentials(id, new CredentialsDto(client.Credentials.Password, Scope));
                }

                _clients[id] = client;
            }

            return client;
        }
    }

    private CredentialsDto? ReadCredentials(Guid id)
    {
        try
        {
            using var key = _store.CreatePluginKey(GitHubPlugin.Id);
            using var subKey = key.OpenSubKey("Credentials");

            if (subKey?.GetValue(id.ToString()) is string protectedCredentials)
            {
                var credentialsBytes = ProtectedData.Unprotect(
                    Convert.FromBase64String(protectedCredentials),
                    null,
                    DataProtectionScope.CurrentUser
                );

                using var stream = new MemoryStream(credentialsBytes);

                return JsonSerializer.Deserialize<CredentialsDto>(stream);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load credentials for connection {ConnectionId}", id);
        }

        return null;
    }

    private void WriteCredentials(Guid id, CredentialsDto credentialsDto)
    {
        using var stream = new MemoryStream();

        JsonSerializer.Serialize(stream, credentialsDto);

        var protectedCredentials = Convert.ToBase64String(
            ProtectedData.Protect(stream.ToArray(), null, DataProtectionScope.CurrentUser)
        );

        using var key = _store.CreatePluginKey(GitHubPlugin.Id);
        using var subKey = key.CreateSubKey("Credentials")!;

        subKey.SetValue(id.ToString(), protectedCredentials);
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
