# Create a runnable match

We now have everything we need to start implementing our search functionality,
but we can't yet return anything. So, before we implement search, we'll add
everything we need to be able to return search some search results.

Start by adding a new class called **PackageMatch** and paste in the following
code:

```cs
using System.Windows.Media;
using Tql.Abstractions;

namespace TqlNuGetPlugin;

internal class PackageMatch : IRunnableMatch
{
    private readonly PackageDto _dto;

    public string Text => _dto.PackageId;
    public ImageSource Icon => Images.NuGetLogo;
    public MatchTypeId TypeId => TypeIds.Package;

    public PackageMatch(PackageDto dto)
    {
        _dto = dto;
    }

    public Task Run(IServiceProvider serviceProvider, IWin32Window owner)
    {
        throw new NotImplementedException();
    }
}

internal record PackageDto(string PackageId);
```

It's best practice to define DTO objects for all matches. DTO object should be
immutable, so the C# `record` type is a good fit. Because our project is a .NET
Framework project, we need to add a missing attribute to our class project. Add
a new file called **CompilerServices** and paste in the following content:

```cs
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
```

This allows us to use `record` types.

We also have to add a new type ID to the `TypeIds` class. Add the following code
in it:

```cs
public static readonly MatchTypeId Package = new MatchTypeId(
    Guid.Parse("188fcd76-a9b9-485e-a0ae-bd5da448a668"),
    Plugin.PluginId
);
```

That's it for now. We'll come back to this once we've implemented search.
