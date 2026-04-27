using KLCMC.Pos.Core.Data;
using KLCMC.Pos.Core.Data.Repositories;
using KLCMC.Pos.Core.Services;
using KLCMC.Pos.Core.ViewModels;
using KLCMC.Pos.Printer.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Windows;

namespace KLCMC.Pos.App;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        using (var scope = _serviceProvider.CreateScope())
        {
            var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<PosDbContext>>();
            using var db = factory.CreateDbContext();
            DatabaseInitializer.EnsureReady(db);
        }

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "com.klcmc.pos");
        Directory.CreateDirectory(appDataPath);
        var dbPath = Path.Combine(appDataPath, "klcmcpos.db");

        services.AddLogging(logging => logging.AddDebug());

        services.AddDbContextFactory<PosDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        services.AddSingleton<IProductRepository, ProductRepository>();
        services.AddSingleton<ISaleRepository, SaleRepository>();
        services.AddSingleton<IPrinterSettingsRepository, PrinterSettingsRepository>();
        services.AddSingleton<IPrinterService, PosDllPrinterService>();

        services.AddSingleton<MainViewModel>();
        services.AddSingleton<DailyAccountViewModel>();
        services.AddSingleton<MainWindow>();
    }
}
