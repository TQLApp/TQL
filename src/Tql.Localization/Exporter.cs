using System.Diagnostics;
using System.Text.RegularExpressions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Tql.Localization;

internal class Exporter(Options options) : Tool(options)
{
    public static readonly ImmutableArray<string> Headers = ImmutableArray.Create(
        "Project",
        "Key",
        "Default Value",
        "Localized Value",
        "Comment"
    );

    public int Run()
    {
        if (ReportUnusedResourceStrings())
        {
            LogError("There are unused resource strings");
            return 1;
        }

        if (File.Exists(Options.FileName))
        {
            LogError("Export file already exists");
            return 2;
        }

        var resourceStrings = GetResourceStrings();

        if (resourceStrings.All(p => !string.IsNullOrEmpty(p.LocalizedValue)))
        {
            LogInfo("Nothing to translate");
            return 0;
        }

        if (!Options.IsAll)
            resourceStrings.RemoveAll(p => !string.IsNullOrEmpty(p.LocalizedValue));

        WriteWorkbook(resourceStrings);

        LogWarning("Found untranslated strings");

        Process.Start(new ProcessStartInfo { FileName = Options.FileName, UseShellExecute = true });

        return 3;
    }

    private bool ReportUnusedResourceStrings()
    {
        var anyUnused = false;

        foreach (var resource in GetAllResources())
        {
            var baseResourceStrings = ReadResourceFile(resource.FileName).ToList();

            var unused = ReportUnusedResourceStrings(
                baseResourceStrings.Select(p => p.Key).ToList(),
                resource.FileName
            );

            anyUnused = anyUnused || unused;
        }

        return anyUnused;
    }

    private bool ReportUnusedResourceStrings(List<string> keys, string resourceFileName)
    {
        var projectFolder = GetProjectFolder(resourceFileName);
        var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".cs", ".xaml" };

        var resourceClass = Regex.Escape(Path.GetFileNameWithoutExtension(resourceFileName));
        var re = new Regex($@"{resourceClass}\.([A-Za-z_][A-Za-z0-9_]*)");

        var seen = new HashSet<string>();

        foreach (
            var fileName in Directory
                .EnumerateFiles(projectFolder, "*", SearchOption.AllDirectories)
                .Where(p => extensions.Contains(Path.GetExtension(p)))
        )
        {
            foreach (Match match in re.Matches(File.ReadAllText(fileName)))
            {
                seen.Add(match.Groups[1].Value);
            }
        }

        var unused = keys.Where(p => !seen.Contains(p)).ToList();

        foreach (var key in unused)
        {
            LogError($"Warning: resource string '{key}' is not in use", resourceFileName);
        }

        return unused.Count > 0;
    }

    private string GetProjectFolder(string fileName)
    {
        var folder = Path.GetDirectoryName(fileName);

        while (!string.IsNullOrEmpty(folder))
        {
            if (Directory.GetFiles(folder, "*.csproj").Length > 0)
                return folder;
        }

        throw new InvalidOperationException("Cannot find project folder");
    }

    private void WriteWorkbook(List<ResourceString> resourceStrings)
    {
        var workbook = new XSSFWorkbook();
        var sheet = workbook.CreateSheet("TQL Resource Strings");

        var headerFont = workbook.CreateFont();
        headerFont.IsBold = true;

        var headerStyle = workbook.CreateCellStyle();
        headerStyle.SetFont(headerFont);

        SetRow(sheet, 0, headerStyle, Headers.ToArray());

        for (var i = 0; i < resourceStrings.Count; i++)
        {
            var resourceString = resourceStrings[i];

            SetRow(
                sheet,
                i + 1,
                null,
                resourceString.Key.Project,
                resourceString.Key.Key,
                resourceString.Value,
                resourceString.LocalizedValue,
                resourceString.Comment
            );
        }

        using var stream = File.Create(Options.FileName);

        workbook.Write(stream);
    }

    private void SetRow(ISheet sheet, int index, ICellStyle? style, params string?[] values)
    {
        var row = sheet.CreateRow(index);

        for (var i = 0; i < values.Length; i++)
        {
            var value = values[i];

            if (!string.IsNullOrEmpty(value))
            {
                var cell = row.CreateCell(i);
                if (style != null)
                    cell.CellStyle = style;

                cell.SetCellValue(value);
            }
        }
    }
}
