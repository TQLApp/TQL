using CommandLine;

namespace Tql.Localization;

internal class Options
{
    [Option("import", HelpText = "Import localized resource strings")]
    public bool Import { get; set; }

    [Option("export", HelpText = "Export resource strings")]
    public bool Export { get; set; }

    [Option("locale", Required = true, HelpText = "Locale to export or import")]
    public string Locale { get; set; } = default!;

    [Option("filename", Required = true, HelpText = "File name")]
    public string FileName { get; set; } = default!;
}
