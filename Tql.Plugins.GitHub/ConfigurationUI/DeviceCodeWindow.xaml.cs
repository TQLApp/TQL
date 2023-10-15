using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.ExceptionServices;
using System.Text.Json.Serialization;
using System.Windows.Threading;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Clipboard = System.Windows.Clipboard;

namespace Tql.Plugins.GitHub.ConfigurationUI;

internal partial class DeviceCodeWindow
{
    private readonly DeviceCodeLoginDto _loginDto;
    private readonly string _clientId;
    private readonly IUI _ui;
    private readonly HttpClient _httpClient;
    private readonly DispatcherTimer _timer;
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    public string? AccessToken { get; private set; }
    public ExceptionDispatchInfo? Exception { get; private set; }

    public DeviceCodeWindow(
        DeviceCodeLoginDto loginDto,
        string clientId,
        IUI ui,
        HttpClient httpClient
    )
    {
        _loginDto = loginDto;
        _clientId = clientId;
        _ui = ui;
        _httpClient = httpClient;

        InitializeComponent();

        _userCode.Text = loginDto.UserCode;
        _logo.Source = Images.GitHub;
        _copyImage.Source = Images.Copy;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(loginDto.Interval) };
        _timer.Tick += _timer_Tick;
        _timer.Start();

        Closed += (_, _) => _timer.Stop();
    }

    private async void _timer_Tick(object sender, EventArgs e)
    {
        try
        {
            if (_stopwatch.Elapsed.TotalSeconds > _loginDto.ExpiresIn)
                throw new GitHubAuthenticationException("User code expired");

            // Grant type is taken from https://docs.github.com/en/apps/oauth-apps/building-oauth-apps/authorizing-oauth-apps#device-flow.

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://github.com/login/oauth/access_token"
                    + $"?client_id={Uri.EscapeDataString(_clientId)}"
                    + $"&device_code={Uri.EscapeDataString(_loginDto.DeviceCode)}"
                    + $"&grant_type={Uri.EscapeDataString("urn:ietf:params:oauth:grant-type:device_code")}"
            );

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();

            var dto = JsonSerializer.Deserialize<AccessTokenDto>(stream)!;

            if (dto.Error == "authorization_pending")
                return;

            if (dto.Error == "slow_down")
            {
                _timer.Stop();
                _timer.Interval = TimeSpan.FromSeconds(dto.Interval!.Value);
                _timer.Start();
                return;
            }

            if (dto.Error != null)
                throw new GitHubAuthenticationException(dto.ErrorDescription ?? dto.Error);

            if (dto.TokenType != "bearer")
            {
                throw new GitHubAuthenticationException(
                    $"Did not expect token type '{dto.TokenType}'"
                );
            }

            AccessToken = dto.AccessToken;

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            Exception = ExceptionDispatchInfo.Capture(ex);
            Close();
        }
    }

    private void _openPage_Click(object sender, RoutedEventArgs e)
    {
        _ui.OpenUrl(_loginDto.VerificationUri);
    }

    private void _copy_Click(object sender, RoutedEventArgs e)
    {
        Clipboard.SetText(_userCode.Text);
    }

    private record AccessTokenDto(
        [property: JsonPropertyName("access_token")] string? AccessToken,
        [property: JsonPropertyName("token_type")] string? TokenType,
        [property: JsonPropertyName("scope")] string? Scope,
        [property: JsonPropertyName("error")] string? Error,
        [property: JsonPropertyName("error_description")] string? ErrorDescription,
        [property: JsonPropertyName("error_uri")] string? ErrorUri,
        [property: JsonPropertyName("interval")] int? Interval
    );
}
