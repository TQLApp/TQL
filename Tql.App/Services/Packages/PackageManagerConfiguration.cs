namespace Tql.App.Services.Packages;

internal record PackageManagerConfiguration(ImmutableArray<PackageManagerSource> Sources)
{
    public static readonly PackageManagerConfiguration Default = CreateDefault();

    private static PackageManagerConfiguration CreateDefault()
    {
        return new PackageManagerConfiguration(
            ImmutableArray.Create(
                new PackageManagerSource("https://api.nuget.org/v3/index.json", null, null)
            )
        );
    }

    public static PackageManagerConfiguration FromJson(string? json)
    {
        if (json == null)
            return Default;

        return JsonSerializer.Deserialize<PackageManagerConfiguration>(json)!;
    }

    public string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }
}

internal record PackageManagerSource(string Url, string? UserName, string? ProtectedPassword);
