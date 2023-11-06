namespace Tql.Plugins.GitHub.Categories;

internal record RootItemDto(Guid Id, RootItemScope Scope);

internal enum RootItemScope
{
    Global,
    User
}
