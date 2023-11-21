using System.Reflection;

namespace Tql.App.Services.Packages.AssemblyResolution;

internal record struct AssemblyKey(string Name, string? CultureName)
{
    public static AssemblyKey FromName(AssemblyName name) => new(name.Name, name.CultureName);

    public readonly bool Equals(AssemblyKey other)
    {
        return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase)
            && string.Equals(CultureName, other.CultureName, StringComparison.OrdinalIgnoreCase);
    }

    public override readonly int GetHashCode()
    {
        unchecked
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(Name) * 397
                ^ (
                    CultureName != null
                        ? StringComparer.OrdinalIgnoreCase.GetHashCode(CultureName)
                        : 0
                );
        }
    }
}
