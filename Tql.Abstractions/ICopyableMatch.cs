namespace Tql.Abstractions;

public interface ICopyableMatch : IMatch
{
    Task Copy(IServiceProvider serviceProvider);
}
