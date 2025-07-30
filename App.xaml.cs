using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AsyncWorkersApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private IHost? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        // Create the host with dependency injection
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Register all worker services
                services.AddHostedService<WorkerHostedService>();
                services.AddSingleton<MainWindow>();
                services.AddSingleton<IWorkerLogger, WorkerLogger>();
            })
            .Build();

        // Start the host
        _host.StartAsync();

        // Get the MainWindow from DI and show it
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host?.StopAsync().Wait();
        _host?.Dispose();
        base.OnExit(e);
    }
}

