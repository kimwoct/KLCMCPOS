using KLCMC.Pos.Core.Data;
using KLCMC.Pos.Core.Data.Repositories;
using KLCMC.Pos.Core.Services;
using KLCMC.Pos.Core.ViewModels;
using KLCMC.Pos.Printer.Mock;
#if WINDOWS
using KLCMC.Pos.Printer.Windows;
#endif
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KLCMC.Pos.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "klcmcpos.db");
        builder.Services.AddDbContextFactory<PosDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        builder.Services.AddSingleton<IProductRepository, ProductRepository>();
        builder.Services.AddSingleton<ISaleRepository, SaleRepository>();
        builder.Services.AddSingleton<IPrinterSettingsRepository, PrinterSettingsRepository>();
        builder.Services.AddSingleton<IPaymentMethodRepository, PaymentMethodRepository>();

#if WINDOWS
        builder.Services.AddSingleton<IPrinterService, PosDllPrinterService>();
#else
        builder.Services.AddSingleton<IPrinterService, MockPrinterService>();
#endif
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddTransient<DailyAccountViewModel>();
        builder.Services.AddTransient<DailyAccountPage>();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<PosDbContext>>();
            using var db = factory.CreateDbContext();
            DatabaseInitializer.EnsureReady(db);
        }

        return app;
    }
}
