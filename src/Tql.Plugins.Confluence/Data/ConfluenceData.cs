namespace Tql.Plugins.Confluence.Data;

internal record ConfluenceData(ImmutableArray<ConfluenceConnection> Connections)
{
    public ConfluenceConnection GetConnection(string url) => Connections.Single(p => p.Url == url);
}

internal record ConfluenceConnection(string Url, ImmutableArray<ConfluenceSpace> Spaces);

internal record ConfluenceSpace(string Key, string Name, string Url, string Icon);
