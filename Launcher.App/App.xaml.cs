using Autofac;
using Microsoft.Win32;

namespace Launcher.App;

public partial class App
{
    public static RegistryKey BaseKey => Registry.CurrentUser.CreateSubKey("Software\\Launcher");

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        var builder = new ContainerBuilder();

        BuildContainer(builder);

        var container = builder.Build();

        using var scope = container.BeginLifetimeScope();

        scope.Resolve<MainWindow>().Show();
    }

    private static void BuildContainer(ContainerBuilder builder)
    {
        builder.RegisterType<Settings>().SingleInstance();

        builder.RegisterType<MainWindow>();
    }
}
