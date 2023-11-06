using JetBrains.Annotations;
using Tql.Abstractions;

namespace Tql.Utilities;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors | ImplicitUseTargetFlags.Default)]
public interface IMatchType
{
    Guid Id { get; }

    IMatch? Deserialize(string json);
}
