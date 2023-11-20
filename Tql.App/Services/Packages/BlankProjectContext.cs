using System.Xml.Linq;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Packaging.Signing;
using NuGet.ProjectManagement;

namespace Tql.App.Services.Packages;

internal class BlankProjectContext : INuGetProjectContext
{
    private readonly ILogger _logger;

    public PackageExtractionContext PackageExtractionContext { get; set; }
    public ISourceControlManagerProvider? SourceControlManagerProvider => null;
    public NuGet.ProjectManagement.ExecutionContext? ExecutionContext => null;
    public XDocument? OriginalPackagesConfig { get; set; }
    public NuGetActionType ActionType { get; set; }
    public Guid OperationId { get; set; }

    public BlankProjectContext(ISettings settings, ILogger logger)
    {
        _logger = logger;

        var clientPolicy = ClientPolicyContext.GetClientPolicy(settings, logger);

        PackageExtractionContext = new PackageExtractionContext(
            PackageSaveMode.Defaultv3,
            XmlDocFileSaveMode.None,
            clientPolicy,
            logger
        );
    }

    public FileConflictAction ResolveFileConflict(string message)
    {
        Log(MessageLevel.Warning, message);

        return FileConflictAction.Ignore;
    }

    public void ReportError(string message) => Log(MessageLevel.Error, message);

    public void Log(MessageLevel level, string message, params object[] args)
    {
        var logLevel = level switch
        {
            MessageLevel.Info => LogLevel.Information,
            MessageLevel.Warning => LogLevel.Warning,
            MessageLevel.Debug => LogLevel.Debug,
            MessageLevel.Error => LogLevel.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };

        Log(new LogMessage(logLevel, string.Format(message, args)));
    }

    public void Log(ILogMessage message)
    {
        _logger.Log(message);
    }

    public void ReportError(ILogMessage message)
    {
        Log(message);
    }
}
