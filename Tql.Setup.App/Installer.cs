using System.Diagnostics;
using System.IO;
using Tql.Setup.App.Support;
using Path = System.IO.Path;

namespace Tql.Setup.App;

internal class Installer
{
    private const string AppFileName = "Tql.App.exe";
    private const string AppName = "Techie's Quick Launcher";

    private readonly SynchronizationContext _synchronizationContext =
        SynchronizationContext.Current;

    public event EventHandler? ProgressChanged;

    public double Progress { get; private set; }
    public bool Done { get; private set; }
    public Exception? Exception { get; private set; }

    public void Start()
    {
        var thread = new Thread(ThreadProc) { IsBackground = true };

        thread.Start();
    }

    private void ThreadProc()
    {
        var binFolder = default(string);
        var tmpBinFolder = default(string);

        try
        {
            var baseFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "TQL"
            );

            var update = Directory.Exists(baseFolder);

            binFolder = Path.Combine(baseFolder, "Bin");
            tmpBinFolder = binFolder + "~SETUP";

            RemoveBinFolder(binFolder, tmpBinFolder);

            CopyFiles(binFolder);

            var target = Path.Combine(binFolder, AppFileName);

            if (!update)
                CreateShortcuts(target);

            RunApp(target);
        }
        catch (Exception ex)
        {
            try
            {
                if (binFolder != null && tmpBinFolder != null)
                    RestoreBinFolder(binFolder, tmpBinFolder);
            }
            catch
            {
                // Ignore.
            }

            SetProgress(exception: ex);
        }
        finally
        {
            SetProgress(progress: 1, done: true);
        }
    }

    private void RemoveBinFolder(string binFolder, string tmpBinFolder)
    {
        if (Directory.Exists(tmpBinFolder))
            DirectoryEx.ForceDeleteRecursive(tmpBinFolder);

        if (!Directory.Exists(binFolder))
            return;

        Retry(() =>
        {
            // Renaming the bin folder before deleting it ensures
            // that we own the Bin folder.

            try
            {
                Directory.Move(binFolder, tmpBinFolder);
            }
            catch
            {
                // If that failed, kill the app.

                foreach (
                    var process in Process.GetProcessesByName(
                        Path.GetFileNameWithoutExtension(AppFileName)
                    )
                )
                {
                    process.CloseMainWindow();
                    process.WaitForExit((int)TimeSpan.FromSeconds(1).TotalMilliseconds);
                    process.Kill();
                }

                throw;
            }
        });
    }

    private void CopyFiles(string binFolder)
    {
        var assetsFolder = Path.Combine(
            Path.GetDirectoryName(GetType().Assembly.Location)!,
            "Assets"
        );

        var fileNames = Directory.GetFiles(assetsFolder, "*", SearchOption.AllDirectories);

        for (var i = 0; i < fileNames.Length; i++)
        {
            var fileName = fileNames[i];
            if (!fileName.StartsWith(assetsFolder))
                throw new InvalidOperationException();

            var source = fileName
                .Substring(assetsFolder.Length)
                .TrimStart(Path.DirectorySeparatorChar);

            var directory = Path.GetDirectoryName(source)!;

            var targetDirectory = Path.Combine(binFolder, directory);
            Directory.CreateDirectory(targetDirectory);

            var target = Path.Combine(targetDirectory, Path.GetFileName(source));

            File.Copy(fileName, target, false);

            SetProgress((i + 1.0) / fileNames.Length);
        }
    }

    private void CreateShortcuts(string target)
    {
        var startMenuFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Programs),
            AppName
        );

        var folders = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
            startMenuFolder,
            Environment.GetFolderPath(Environment.SpecialFolder.Startup)
        };

        foreach (var folder in folders)
        {
            CreateShortcut(Path.Combine(folder, $"{AppName}.lnk"), target, 0, null, AppName);
        }

        CreateShortcut(
            Path.Combine(startMenuFolder, "Uninstall.lnk"),
            target,
            0,
            "/uninstall",
            AppName
        );
    }

    private void CreateShortcut(
        string shortcutFileName,
        string target,
        int? iconIndex,
        string? startOptions,
        string? description
    )
    {
        var shellLink = new ShellLink { Target = target };

        if (iconIndex != null)
            shellLink.IconIndex = iconIndex.Value;

        if (startOptions != null)
            shellLink.Arguments = startOptions;

        if (description != null)
            shellLink.Description = description;

        Directory.CreateDirectory(Path.GetDirectoryName(shortcutFileName)!);

        shellLink.Save(shortcutFileName);
    }

    private void RunApp(string target)
    {
        Process.Start(target);
    }

    private void RestoreBinFolder(string binFolder, string tmpBinFolder)
    {
        if (!Directory.Exists(tmpBinFolder))
            return;

        if (Directory.Exists(binFolder))
            DirectoryEx.ForceDeleteRecursive(binFolder);

        Directory.Move(tmpBinFolder, binFolder);
    }

    private void Retry(Action action, int tries = 5)
    {
        for (var remaining = tries; ; remaining--)
        {
            try
            {
                action();
                return;
            }
            catch when (remaining > 0)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }
    }

    private void SetProgress(
        double? progress = null,
        bool? done = null,
        Exception? exception = null
    )
    {
        _synchronizationContext.Post(
            _ =>
            {
                if (progress.HasValue)
                    Progress = progress.Value;
                if (done.HasValue)
                    Done = done.Value;
                if (exception != null)
                    Exception = exception;
                OnProgressChanged();
            },
            null
        );
    }

    protected virtual void OnProgressChanged() => ProgressChanged?.Invoke(this, EventArgs.Empty);
}
