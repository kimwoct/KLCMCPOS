using System;
using System.IO;
using System.Windows;
using KLCMC.Pos.Core.Data;
using KLCMC.Pos.Core.Data.Repositories;
using KLCMC.Pos.Core.ViewModels;
using KLCMC.Pos.Printer.Windows;
using Microsoft.EntityFrameworkCore;

namespace KLCMC.Pos.App;

public partial class MainWindow : Window
{
    private readonly ISaleRepository _saleRepository;
    private readonly PosDllPrinterService _printerService;

    public MainWindow()
    {
        InitializeComponent();

        var dbDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "com.klcmc.pos");
        Directory.CreateDirectory(dbDir);
        var dbPath = Path.Combine(dbDir, "klcmcpos.db");

        var options = new DbContextOptionsBuilder<PosDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;

        var factory = new SimpleDbContextFactory(options);
        using (var initDb = factory.CreateDbContext())
        {
            DatabaseInitializer.EnsureReady(initDb);
        }

        var productRepo = new ProductRepository(factory);
        _saleRepository = new SaleRepository(factory);
        var settingsRepo = new PrinterSettingsRepository(factory);
        _printerService = new PosDllPrinterService();

        DataContext = new MainViewModel(
            _printerService,
            productRepo,
            _saleRepository,
            settingsRepo);
    }

    private void OnDailyAccountClick(object sender, RoutedEventArgs e)
    {
        var vm = new DailyAccountViewModel(_saleRepository, _printerService);
        var window = new DailyAccountWindow(vm) { Owner = this };
        window.Show();
    }
}
