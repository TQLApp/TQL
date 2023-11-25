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

    public void Run()
    {
        if (File.Exists(Options.FileName))
            throw new InvalidOperationException("Export file already exists");

        var resourceStrings = GetResourceStrings();

        WriteWorkbook(resourceStrings);
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
