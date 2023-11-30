using System.Collections;
using System.ComponentModel.Design;
using System.Resources;

namespace Tql.Localization;

internal abstract class Tool
{
    protected Options Options { get; }
    protected string BasePath { get; }
    protected bool HadError { get; private set; }

    protected Tool(Options options)
    {
        Options = options;
        BasePath = ResolveBasePath();
    }

    private string ResolveBasePath()
    {
        var path = Path.GetDirectoryName(GetType().Assembly.Location);

        while (!string.IsNullOrEmpty(path))
        {
            if (File.Exists(Path.Combine(path, "Tql.sln")))
                return path;
            path = Path.GetDirectoryName(path);
        }

        throw new InvalidOperationException("Cannot resolve root folder");
    }

    protected IEnumerable<Resource> GetAllResources()
    {
        foreach (
            var fileName in Directory.GetFiles(BasePath, "Labels.resx", SearchOption.AllDirectories)
        )
        {
            var resources = ImmutableArray.CreateBuilder<LocalizedResource>();

            foreach (
                var resourceFileName in Directory.GetFiles(
                    Path.GetDirectoryName(fileName)!,
                    "Labels.*.resx"
                )
            )
            {
                var parts = Path.GetFileName(resourceFileName).Split('.');
                if (parts.Length != 3)
                    throw new InvalidOperationException("Invalid resource file name");

                resources.Add(new LocalizedResource(resourceFileName, parts[1]));
            }

            var directoryName = Path.GetDirectoryName(fileName)!;
            if (!directoryName.StartsWith(BasePath))
                throw new InvalidOperationException();
            var project = directoryName
                .Substring(BasePath.Length)
                .Trim(Path.DirectorySeparatorChar);

            yield return new Resource(project, fileName, resources.ToImmutable());
        }
    }

    protected List<ResourceString> GetResourceStrings()
    {
        var resourceStrings = new List<ResourceString>();

        foreach (var resource in GetAllResources())
        {
            var baseResourceStrings = ReadResourceFile(resource.FileName).ToList();

            var localizedResource = resource
                .Resources
                .SingleOrDefault(
                    p => string.Equals(p.Locale, Options.Locale, StringComparison.OrdinalIgnoreCase)
                );
            var localizedResourceStrings = ReadResourceFile(localizedResource?.FileName)
                .ToDictionary(p => p.Key, p => p);

            foreach (var resourceString in baseResourceStrings)
            {
                var comment = resourceString.Comment;
                if (string.IsNullOrEmpty(comment))
                    comment = null;

                var localizedResourceStringExists = localizedResourceStrings.TryGetValue(
                    resourceString.Key,
                    out var localizedResourceString
                );

                if (localizedResourceStringExists)
                {
                    var localizedComment = localizedResourceString.Comment;
                    if (string.IsNullOrEmpty(localizedComment))
                    {
                        LogWarning(
                            $"Warning: comment of localized resource '{resourceString.Key}' is empty",
                            localizedResource?.FileName!
                        );
                    }
                    else
                    {
                        const string prefix = "Original: ";
                        if (!localizedComment.StartsWith(prefix))
                        {
                            LogWarning(
                                $"Warning: comment of localized resource '{resourceString.Key}' should start with '{prefix}'",
                                localizedResource?.FileName!
                            );
                        }
                        else
                        {
                            var originalValue = localizedComment.Substring(prefix.Length);
                            if (originalValue != resourceString.Value)
                            {
                                LogWarning(
                                    $"Warning: ignoring localized value of resource '{resourceString.Key}' because it's a translation off of a different source string",
                                    localizedResource?.FileName!
                                );
                                localizedResourceString = default;
                            }
                        }
                    }
                }

                resourceStrings.Add(
                    new ResourceString(
                        new ResourceKey(resource.Project, resourceString.Key),
                        resourceString.Value,
                        localizedResourceString.Value,
                        comment
                    )
                );
            }
        }

        return resourceStrings;
    }

    protected static IEnumerable<(string Key, string Value, string Comment)> ReadResourceFile(
        string? fileName
    )
    {
        if (fileName == null || !File.Exists(fileName))
            yield break;

        using var stream = File.OpenRead(fileName);
        using var reader = new ResXResourceReader(stream);

        reader.UseResXDataNodes = true;

        foreach (DictionaryEntry entry in reader)
        {
            var data = (ResXDataNode?)entry.Value;
            if (data?.GetValue((ITypeResolutionService)null!) is string value)
            {
                yield return (Key: (string)entry.Key, Value: value, data.Comment);
            }
        }
    }

    protected void LogError(string message, string? fileName = null)
    {
        Log(LogLevel.Error, message, fileName);
        HadError = true;
    }

    protected void LogWarning(string message, string? fileName = null) =>
        Log(LogLevel.Warning, message, fileName);

    protected void LogInfo(string message, string? fileName = null) =>
        Log(LogLevel.Info, message, fileName);

    private void Log(LogLevel level, string message, string? fileName)
    {
        var sb = new StringBuilder();

        if (Options.IsPipeline)
        {
            sb.Append("::")
                .Append(
                    level switch
                    {
                        LogLevel.Error => "error",
                        LogLevel.Warning => "warning",
                        LogLevel.Info => "notice",
                        _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
                    }
                );

            if (fileName != null)
            {
                sb.Append(" file=").Append(fileName);
            }

            sb.Append(":: ").Append(message);
        }
        else
        {
            if (fileName != null)
                sb.Append('[').Append(fileName).Append("] ");

            sb.Append(level).Append(": ").Append(message);
        }

        if (level == LogLevel.Error)
            Console.Error.WriteLine(sb.ToString());
        else
            Console.WriteLine(sb.ToString());
    }

    private enum LogLevel
    {
        Error,
        Warning,
        Info
    }
}

internal record Resource(
    string Project,
    string FileName,
    ImmutableArray<LocalizedResource> Resources
);

internal record LocalizedResource(string FileName, string Locale);

internal record struct ResourceKey(string Project, string Key);

internal record ResourceString(
    ResourceKey Key,
    string Value,
    string? LocalizedValue,
    string? Comment
);
