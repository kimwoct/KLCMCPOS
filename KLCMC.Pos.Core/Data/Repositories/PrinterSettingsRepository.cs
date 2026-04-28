using KLCMC.Pos.Core.Data.Entities;
using KLCMC.Pos.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace KLCMC.Pos.Core.Data.Repositories;

public sealed class PrinterSettingsRepository : IPrinterSettingsRepository
{
    private readonly IDbContextFactory<PosDbContext> _dbFactory;

    public PrinterSettingsRepository(IDbContextFactory<PosDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public PrinterConnectionOptions Load()
    {
        using var db = _dbFactory.CreateDbContext();
        var entity = db.PrinterSettings.Find(1);
        if (entity is null)
        {
            entity = new PrinterSettingEntity
            {
                Id = 1,
                Mode = PrinterConnectionMode.Usb,
                Endpoint = "POS-80 11.3.0.1",
                BaudRate = 9600,
                DataBits = 8,
                StopBits = 0,
                Parity = 0,
                FlowControl = 1,
                PaperWidthMm = 80,
                CodePage = "UTF-8",
                CutMode = PrinterCutMode.Partial,
                DrawerPulseOnMs = 120,
                DrawerPulseOffMs = 240
            };
            db.PrinterSettings.Add(entity);
            db.SaveChanges();
        }
        else if (entity.Mode == PrinterConnectionMode.Usb &&
                 string.Equals(entity.Endpoint, "USB001", StringComparison.OrdinalIgnoreCase))
        {
            entity.Endpoint = "POS-80 11.3.0.1";
            db.SaveChanges();
        }

        return new PrinterConnectionOptions
        {
            Mode = entity.Mode,
            Endpoint = entity.Endpoint,
            BaudRate = entity.BaudRate,
            DataBits = entity.DataBits,
            StopBits = entity.StopBits,
            Parity = entity.Parity,
            FlowControl = entity.FlowControl,
            PaperWidthMm = entity.PaperWidthMm <= 0 ? 80 : entity.PaperWidthMm,
            CodePage = string.IsNullOrWhiteSpace(entity.CodePage) ? "UTF-8" : entity.CodePage,
            CutMode = entity.CutMode,
            DrawerPulseOnMs = entity.DrawerPulseOnMs <= 0 ? 120 : entity.DrawerPulseOnMs,
            DrawerPulseOffMs = entity.DrawerPulseOffMs <= 0 ? 240 : entity.DrawerPulseOffMs
        };
    }

    public void Save(PrinterConnectionOptions options)
    {
        using var db = _dbFactory.CreateDbContext();
        var entity = db.PrinterSettings.Find(1);
        if (entity is null)
        {
            entity = new PrinterSettingEntity { Id = 1 };
            db.PrinterSettings.Add(entity);
        }

        entity.Mode = options.Mode;
        entity.Endpoint = options.Endpoint;
        entity.BaudRate = options.BaudRate;
        entity.DataBits = options.DataBits;
        entity.StopBits = options.StopBits;
        entity.Parity = options.Parity;
        entity.FlowControl = options.FlowControl;
        entity.PaperWidthMm = options.PaperWidthMm;
        entity.CodePage = options.CodePage;
        entity.CutMode = options.CutMode;
        entity.DrawerPulseOnMs = options.DrawerPulseOnMs;
        entity.DrawerPulseOffMs = options.DrawerPulseOffMs;
        db.SaveChanges();
    }
}
