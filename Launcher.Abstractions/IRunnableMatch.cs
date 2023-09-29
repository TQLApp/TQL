using System.Windows;

namespace Launcher.Abstractions;

public interface IRunnableMatch
{
    Task Run(IServiceProvider serviceProvider, Window owner);
}
