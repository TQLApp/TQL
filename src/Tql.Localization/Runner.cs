namespace Tql.Localization;

internal class Runner(Options options)
{
    public int Run()
    {
        if (options.Export)
            return new Exporter(options).Run();
        else if (options.Import)
            return new Importer(options).Run();
        else
            throw new InvalidOperationException("Either export or import must be specified");
    }
}
