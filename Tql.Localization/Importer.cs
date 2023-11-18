using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Resources;
using System.Text.RegularExpressions;

namespace Tql.Localization;

internal class Importer : Tool
{
    private bool _error;

    public Importer(Options options)
        : base(options) { }

    public void Run()
    {
        var newResourceStrings = ParseWorkbook();

        if (_error)
            return;

        var resourceStrings = GetResourceStrings();
        var updatedComments = GetUpdatedComments(resourceStrings, newResourceStrings);

        if (_error)
            return;

        WriteUpdatedComments(updatedComments, resourceStrings);

        WriteUpdatedResources(newResourceStrings, resourceStrings);
    }

    private void WriteUpdatedResources(
        List<(int Row, ResourceString String)> newResourceStrings,
        List<ResourceString> resourceStrings
    )
    {
        var newResourceStringMap = newResourceStrings.ToDictionary(
            p => p.String.Key,
            p => p.String
        );

        foreach (var project in resourceStrings.Select(p => p.Key.Project).Distinct())
        {
            using var stream = File.Create(
                Path.Combine(BasePath, project, $"Labels.{Options.Locale}.resx")
            );
            using var writer = new ResXResourceWriter(stream);

            foreach (var entry in resourceStrings.Where(p => p.Key.Project == project))
            {
                var localizedValue = entry.LocalizedValue;
                if (newResourceStringMap.TryGetValue(entry.Key, out var newResourceString))
                    localizedValue = newResourceString.LocalizedValue;

                writer.AddResource(
                    new ResXDataNode(entry.Key.Key, localizedValue ?? string.Empty)
                    {
                        Comment = $"Original: {entry.Value}"
                    }
                );
            }
        }
    }

    private void WriteUpdatedComments(
        Dictionary<ResourceKey, string?> updatedComments,
        List<ResourceString> resourceStrings
    )
    {
        foreach (var project in updatedComments.Select(p => p.Key.Project).Distinct())
        {
            using var stream = File.Create(Path.Combine(BasePath, project, "Labels.resx"));
            using var writer = new ResXResourceWriter(stream);

            foreach (var entry in resourceStrings.Where(p => p.Key.Project == project))
            {
                if (!updatedComments.TryGetValue(entry.Key, out var comment))
                    comment = entry.Comment;

                var entryValue = entry.Value;
                // Entry value can be null (even though the type is defined as
                // not nullable).
                if (string.IsNullOrEmpty(entryValue))
                    entryValue = string.Empty;

                writer.AddResource(
                    new ResXDataNode(entry.Key.Key, entryValue) { Comment = comment }
                );
            }
        }
    }

    private Dictionary<ResourceKey, string?> GetUpdatedComments(
        List<ResourceString> resourceStrings,
        List<(int Row, ResourceString String)> newResourceStrings
    )
    {
        var updatedComments = new Dictionary<ResourceKey, string?>();
        var resourceStringMap = resourceStrings.ToDictionary(p => p.Key, p => p);

        foreach (var (row, newResourceString) in newResourceStrings)
        {
            if (!resourceStringMap.TryGetValue(newResourceString.Key, out var resourceString))
            {
                WriteError(row, $"Unknown key '{newResourceString.Key.Key}'");
                continue;
            }

            if (newResourceString.Value != resourceString.Value)
            {
                WriteError(
                    row,
                    $"Unlocalized value '{newResourceString.Value}' differs from '{resourceString.Value}'"
                );
                continue;
            }

            if (newResourceString.Comment != resourceString.Comment)
                updatedComments.Add(newResourceString.Key, newResourceString.Comment);
        }

        return updatedComments;
    }

    private List<(int Row, ResourceString String)> ParseWorkbook()
    {
        using var stream = File.OpenRead(Options.FileName);

        var workbook = new XSSFWorkbook(stream);
        var sheet = workbook.GetSheetAt(0);

        var headers = GetRow(sheet, 0, 5);
        if (!ArrayEquals(headers, Exporter.Headers.ToArray()))
            throw new InvalidOperationException("Unexpected Excel file");

        var newResourceStrings = new List<(int Row, ResourceString String)>();

        for (var i = 1; i <= sheet.LastRowNum; i++)
        {
            var values = GetRow(sheet, i, 5);

            if (values[0] == null)
            {
                WriteError(i, "Missing project");
                continue;
            }

            if (values[1] == null)
            {
                WriteError(i, "Missing key");
                continue;
            }

            if (values[2] == null)
            {
                WriteError(i, "Missing untranslated value");
                continue;
            }

            var resourceString = new ResourceString(
                new ResourceKey(values[0]!, values[1]!),
                ReplaceNewlines(values[2])!,
                ReplaceNewlines(values[3]),
                values[4]
            );

            newResourceStrings.Add((i, resourceString));
        }

        return newResourceStrings;
    }

    private void WriteError(int row, string message)
    {
        Console.Error.WriteLine($"Row {row + 1}: {message}");
        _error = true;
    }

    private bool ArrayEquals(string?[] a, string?[] b)
    {
        if (a.Length != b.Length)
            return false;

        for (var i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i])
                return false;
        }

        return true;
    }

    private string?[] GetRow(ISheet sheet, int index, int cells)
    {
        var values = new string?[cells];

        var row = sheet.GetRow(index);
        if (row != null)
        {
            for (var i = 0; i < cells; i++)
            {
                var cell = row.GetCell(i);
                var value = cell?.StringCellValue;
                if (!string.IsNullOrEmpty(value))
                    values[i] = value;
            }
        }

        return values;
    }

    private string? ReplaceNewlines(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return Regex.Replace(value, "\r?\n", "\r\n");
    }
}
