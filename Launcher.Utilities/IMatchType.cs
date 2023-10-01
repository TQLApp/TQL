using Launcher.Abstractions;

namespace Launcher.Utilities;

public interface IMatchType
{
    Guid Id { get; }

    IMatch Deserialize(string text, string json);
}
