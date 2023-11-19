using CommandLine;

namespace Tql.App;

internal class Options
{
    [Option("silent")]
    public bool IsSilent { get; set; }

    [Option("env")]
    public string? Environment { get; set; }

    [Option("sideload")]
    public string? Sideload { get; set; }
}
