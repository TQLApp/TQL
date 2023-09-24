using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Win32;

namespace Launcher.App;

public partial class App
{
    public static RegistryKey BaseKey => Registry.CurrentUser.CreateSubKey("Software\\Launcher");

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        var builder = Host.CreateApplicationBuilder(e.Args);

        BuildContainer(builder.Services);

        using var host = builder.Build();

        using var scope = host.Services.CreateScope();

        scope.ServiceProvider.GetRequiredService<MainWindow>().Show();
    }

    private static void BuildContainer(IServiceCollection builder)
    {
        builder.AddSingleton<Settings>();

        builder.AddScoped<MainWindow>();
    }
}
