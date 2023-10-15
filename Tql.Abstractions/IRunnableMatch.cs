using System.Windows;

namespace Tql.Abstractions;

public interface IRunnableMatch : IMatch
{
    Task Run(IServiceProvider serviceProvider, Window owner);
}
