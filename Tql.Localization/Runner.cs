namespace Tql.Localization;

internal class Runner
{
    private readonly Options _options;

    public Runner(Options options)
    {
        _options = options;
    }

    public void Run()
    {
        if (_options.Export)
            new Exporter(_options).Run();
        else if (_options.Import)
            new Importer(_options).Run();
        else
            throw new InvalidOperationException("Either export or import must be specified");
    }
}
