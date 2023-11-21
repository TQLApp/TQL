namespace Tql.Localization;

internal class Runner(Options options)
{
    public void Run()
    {
        if (options.Export)
            new Exporter(options).Run();
        else if (options.Import)
            new Importer(options).Run();
        else
            throw new InvalidOperationException("Either export or import must be specified");
    }
}
