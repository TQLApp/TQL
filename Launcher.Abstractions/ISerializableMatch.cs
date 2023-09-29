namespace Launcher.Abstractions;

public interface ISerializableMatch
{
    (Guid TypeId, string Json) Serialize();
}
