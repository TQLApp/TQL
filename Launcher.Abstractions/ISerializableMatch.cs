namespace Launcher.Abstractions;

public interface ISerializableMatch : IMatch
{
    string Serialize();
}
