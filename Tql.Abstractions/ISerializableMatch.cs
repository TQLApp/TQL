namespace Tql.Abstractions;

public interface ISerializableMatch : IMatch
{
    string Serialize();
}
