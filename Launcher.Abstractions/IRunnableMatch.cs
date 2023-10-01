using System.Windows;

namespace Launcher.Abstractions;

public interface IRunnableMatch : IMatch
{
    Task Run(IServiceProvider serviceProvider, Window owner);
}
