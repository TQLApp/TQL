# Adding a type ID

Every match class in TQL is identified by a type ID. These are used e.g. as stable names in the user's history.

It's best practice to create type IDs in a separate class. To do this, create a new class called **TypeIds** and paste in the following code:

```cs
using Tql.Abstractions;

namespace TqlNuGetPlugin;

internal static class TypeIds
{
    public static readonly MatchTypeId Packages = new MatchTypeId(
        Guid.Parse("b1c8f27b-8534-4ee7-bcc6-e45fef01e5bc"),
        Plugin.PluginId
    );
}
```

Type IDs are a combination of the ID of the match and the ID of the plugin. Now, update the `TypeId` property of the match implementation to use the new class:

```cs
public MatchTypeId TypeId => TypeIds.Packages;
```

That's it. This doesn't give us anything new, but it does complete the boilerplate for our match class.

Next step is to create a match class for our search results.