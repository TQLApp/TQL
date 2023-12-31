﻿using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Tql.App.Services.Packages;
using Tql.App.Support;
using Path = System.IO.Path;

namespace Tql.App.Services.Updates;

internal class UpdateChecker : IDisposable
{
    private static readonly TimeSpan UpdateCheckInterval = TimeSpan.FromHours(1);

    private readonly IUI _ui;
    private readonly ILogger<UpdateChecker> _logger;
    private readonly HttpClient _httpClient;
    private readonly PackageManager _packageManager;
    private readonly NotifyIconManager _notifyIconManager;
    private readonly LocalSettings _settings;
    private readonly Timer _timer;

    public UpdateChecker(
        IUI ui,
        ILogger<UpdateChecker> logger,
        HttpClient httpClient,
        PackageManager packageManager,
        NotifyIconManager notifyIconManager,
        LocalSettings settings
    )
    {
        _ui = ui;
        _logger = logger;
        _httpClient = httpClient;
        _packageManager = packageManager;
        _notifyIconManager = notifyIconManager;
        _settings = settings;
        _timer = new Timer(TimerCallback, null, UpdateCheckInterval, UpdateCheckInterval);
    }

    public bool TryStartUpdate(IProgress progress)
    {
        _notifyIconManager.State = NotifyIconState.Updating;

        var result = false;

        try
        {
            result = TaskUtils.RunSynchronously(() => TryStartUpdateAsync(progress));
        }
        finally
        {
            _notifyIconManager.State = NotifyIconState.Running;
        }

        return result;
    }

    private async Task<bool> TryStartUpdateAsync(IProgress progress)
    {
        return await TryStartApplicationUpdate(progress) || await TryStartPluginUpdate(progress);
    }

    private async Task<bool> TryStartApplicationUpdate(IProgress progress)
    {
        var installPrerelease = _settings.InstallPrerelease.GetValueOrDefault();

        var release = installPrerelease ? await GetLatestPrerelease() : await GetLatestRelease();
        if (release == null)
            return false;

        if (
            !release.TagName.StartsWith("v")
            || !Version.TryParse(release.TagName.Substring(1), out var releaseVersion)
        )
            throw new InvalidOperationException("Unexpected tag format");

        var appVersion = GitHubUtils.GetAppVersion();

        _logger.LogInformation(
            "Latest release version {ReleaseVersion}, app version {AppVersion}",
            releaseVersion,
            appVersion
        );

        if (appVersion.CompareTo(releaseVersion) >= 0)
            return false;

        var assets = release
            .Assets.Where(p => p.Name.EndsWith(".msi", StringComparison.OrdinalIgnoreCase))
            .ToList();

        switch (assets.Count)
        {
            case 0:
                _logger.LogError("Release does not have an MSI file");
                return false;
            case 1:
                break;
            default:
                _logger.LogError("Release has more than one MSI files");
                return false;
        }

        progress.SetProgress(Labels.UpdateChecker_DownloadingUpdate, 0);

        var target = await Download(assets.Single());

        progress.SetProgress(Labels.UpdateChecker_InstallingUpdate, 1);

        // Give the user a chance to see the message.
        await Task.Delay(TimeSpan.FromSeconds(0.5));

        Install(target);

        return true;
    }

    private async Task<ReleaseDto?> GetLatestRelease()
    {
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            "https://api.github.com/repos/TQLApp/TQL/releases/latest"
        );

        GitHubUtils.InitializeRequest(request);

        using var response = await _httpClient.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogInformation(
                "Request for latest release returned not found; build is likely going"
            );
            return null;
        }

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<ReleaseDto>(json)!;
    }

    private async Task<ReleaseDto?> GetLatestPrerelease()
    {
        var releases = new List<ReleaseDto>();
        const int pageSize = 100;

        for (var page = 1; ; page++)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://api.github.com/repos/TQLApp/TQL/releases?per_page={pageSize}&page={page}"
            );

            GitHubUtils.InitializeRequest(request);

            using var response = await _httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogInformation(
                    "Request for latest release returned not found; build is likely going"
                );
                return null;
            }

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var pageReleases = JsonSerializer.Deserialize<List<ReleaseDto>>(json)!;

            releases.AddRange(pageReleases);

            if (pageReleases.Count < pageSize)
                break;
        }

        return releases.Where(p => p.Prerelease).MaxBy(p => p.CreatedAt);
    }

    private async Task<bool> TryStartPluginUpdate(IProgress progress)
    {
        var updatesAvailable = await _packageManager.UpdatePlugins(
            progress,
            PackageProgressMode.Update
        );

        if (updatesAvailable)
        {
            _logger.LogInformation("One or more plugins were updated; restarting");

            _ui.Shutdown(App.Options.IsSilent ? RestartMode.SilentRestart : RestartMode.Restart);
        }

        return updatesAvailable;
    }

    private async Task<string> Download(ReleaseAssetDto asset)
    {
        _logger.LogInformation("Downloading setup from {Url}", asset.Url);

        var targetFolder = Path.Combine(
            Path.GetTempPath(),
            $"TqlSetup~{new Random().Next(100000, 999999)}"
        );
        Directory.CreateDirectory(targetFolder);

        var targetFileName = Path.Combine(targetFolder, "Tql.msi");

        using var request = new HttpRequestMessage(HttpMethod.Get, asset.Url);

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

        GitHubUtils.InitializeRequest(request);

        using var response = await _httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        await using var source = await response.Content.ReadAsStreamAsync();
        await using var target = File.Create(targetFileName);

        await source.CopyToAsync(target);

        return targetFileName;
    }

    private void Install(string target)
    {
        _logger.LogInformation("Running setup at '{Target}'", target);

        Process.Start(
            new ProcessStartInfo
            {
                FileName = "msiexec.exe",
                WorkingDirectory = Path.GetDirectoryName(target),
                Arguments = $"/i \"{Path.GetFileName(target)}\" /qn",
            }
        );
    }

    private void TimerCallback(object? state)
    {
        try
        {
            if (TryStartUpdate(NullProgress.Instance))
            {
                _logger.LogInformation("Updating is running; shutting down");

                _ui.Shutdown(RestartMode.SilentRestart);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while checking for updates");
        }
    }

    public void Dispose()
    {
        _timer.Dispose();
    }
}
