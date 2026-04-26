using KLCMC.Pos.Core.Services;
using KLCMC.Pos.Core.ViewModels;
using KLCMC.Pos.Printer.Mock;
#if WINDOWS
using KLCMC.Pos.Printer.Windows;
#endif
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

#if WINDOWS
        builder.Services.AddSingleton<IPrinterService, PosDllPrinterService>();
#else
        builder.Services.AddSingleton<IPrinterService, MockPrinterService>();
#endif
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MainPage>();

        return builder.Build();
    }
}
