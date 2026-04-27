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
                Endpoint = "USB001",
                BaudRate = 9600,
                DataBits = 8,
                StopBits = 0,
                Parity = 0,
                FlowControl = 1
            };
            db.PrinterSettings.Add(entity);
            db.SaveChanges();
        }
        else if (entity.Mode == PrinterConnectionMode.Serial &&
                 string.Equals(entity.Endpoint, "COM1", StringComparison.OrdinalIgnoreCase) &&
                 entity.BaudRate == 115200 &&
                 entity.DataBits == 8 &&
                 entity.StopBits == 0 &&
                 entity.Parity == 0 &&
                 entity.FlowControl == 1)
        {
            entity.Mode = PrinterConnectionMode.Usb;
            entity.Endpoint = "USB001";
            entity.BaudRate = 9600;
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
            FlowControl = entity.FlowControl
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
        db.SaveChanges();
    }
}
