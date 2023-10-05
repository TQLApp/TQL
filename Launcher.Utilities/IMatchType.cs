using JetBrains.Annotations;
using Launcher.Abstractions;

namespace Launcher.Utilities;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors | ImplicitUseTargetFlags.Default)]
public interface IMatchType
{
    Guid Id { get; }

    IMatch Deserialize(string json);
}
