namespace Tql.Plugins.GitHub.Support;

[AttributeUsage(AttributeTargets.Class)]
internal class RootMatchTypeAttribute : Attribute
{
    public bool SupportsUserScope { get; set; }
}
