namespace Tql.App.Services.Packages.PackageStore;

internal record PackageManifest(int Version, ImmutableArray<PackageExport> Entries);

internal record PackageExport(string FileName, string TypeName);
