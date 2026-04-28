using KLCMC.Pos.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace KLCMC.Pos.Core.Data;

public static class DatabaseInitializer
{
    private static readonly (string Name, decimal Price)[] DefaultProducts =
    {
        ("中醫治療 診症", 80.00m),
        ("中醫診治 針灸", 250.00m),
        ("醫療券", 100.00m),
        ("中醫藥", 100.00m),
        ("藥品", 500.00m),
        ("雜項", 1.00m)
    };

    public static void EnsureReady(PosDbContext db)
    {
        db.Database.EnsureCreated();
        EnsurePrinterSettingsSchema(db);
        EnsurePaymentMethodsTable(db);

        if (!db.Products.Any())
        {
            db.Products.AddRange(DefaultProducts.Select(p => new ProductEntity
            {
                Name = p.Name,
                DefaultPrice = p.Price
            }));
            db.SaveChanges();
        }

        if (!db.PrinterSettings.Any())
        {
            db.PrinterSettings.Add(new PrinterSettingEntity
            {
                Id = 1,
                Mode = KLCMC.Pos.Core.Models.PrinterConnectionMode.Usb,
                Endpoint = "POS-80",
                BaudRate = 9600,
                DataBits = 8,
                StopBits = 0,
                Parity = 0,
                FlowControl = 1,
                PaperWidthMm = 80,
                CodePage = "UTF-8",
                CutMode = KLCMC.Pos.Core.Models.PrinterCutMode.Partial,
                DrawerPulseOnMs = 120,
                DrawerPulseOffMs = 240
            });
            db.SaveChanges();
        }

        if (!db.PaymentMethods.Any())
        {
            SeedDefaultPaymentMethods(db);
        }
        else
        {
            MigratePaymentMethodsToTraditionalChinese(db);
        }
    }

    private static readonly Dictionary<string, string> _englishToChineseMethod = new()
    {
        ["Cash"]    = "現金",
        ["Card"]    = "信用卡",
        ["Octopus"] = "八達通",
        ["FPS"]     = "醫療券",   // replaced by clinic-specific
        ["Other"]   = "其他",
    };

    private static void SeedDefaultPaymentMethods(PosDbContext db)
    {
        var defaults = new[] { "現金", "信用卡", "醫療券", "八達通", "其他" };
        for (var i = 0; i < defaults.Length; i++)
            db.PaymentMethods.Add(new PaymentMethodEntity { Name = defaults[i], SortOrder = i });
        db.SaveChanges();
    }

    // One-time migration: rename English defaults to Traditional Chinese
    private static void MigratePaymentMethodsToTraditionalChinese(PosDbContext db)
    {
        var methods = db.PaymentMethods.ToList();
        var changed = false;
        foreach (var m in methods)
        {
            if (_englishToChineseMethod.TryGetValue(m.Name, out var chinese))
            {
                m.Name = chinese;
                changed = true;
            }
        }
        if (changed) db.SaveChanges();
    }

    private static void EnsurePaymentMethodsTable(PosDbContext db)
    {
        db.Database.ExecuteSqlRaw(
            "CREATE TABLE IF NOT EXISTS \"PaymentMethods\" (" +
            "\"Id\" INTEGER NOT NULL CONSTRAINT \"PK_PaymentMethods\" PRIMARY KEY AUTOINCREMENT," +
            "\"Name\" TEXT NOT NULL," +
            "\"IsActive\" INTEGER NOT NULL DEFAULT 1," +
            "\"SortOrder\" INTEGER NOT NULL DEFAULT 0" +
            ");");
    }

    private static void EnsurePrinterSettingsSchema(PosDbContext db)
    {
        using var command = db.Database.GetDbConnection().CreateCommand();
        command.CommandText = "PRAGMA table_info('PrinterSettings');";
        db.Database.OpenConnection();
        using var reader = command.ExecuteReader();
        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (reader.Read())
        {
            columns.Add(reader.GetString(1));
        }

        AddColumnIfMissing(db, columns, "PaperWidthMm");
        AddColumnIfMissing(db, columns, "CodePage");
        AddColumnIfMissing(db, columns, "CutMode");
        AddColumnIfMissing(db, columns, "DrawerPulseOnMs");
        AddColumnIfMissing(db, columns, "DrawerPulseOffMs");
        db.Database.CloseConnection();
    }

    private static void AddColumnIfMissing(PosDbContext db, ISet<string> columns, string columnName)
    {
        if (columns.Contains(columnName))
        {
            return;
        }

        var sql = columnName switch
        {
            "PaperWidthMm" => "ALTER TABLE PrinterSettings ADD COLUMN PaperWidthMm INTEGER NOT NULL DEFAULT 80;",
            "CodePage" => "ALTER TABLE PrinterSettings ADD COLUMN CodePage TEXT NOT NULL DEFAULT 'UTF-8';",
            "CutMode" => "ALTER TABLE PrinterSettings ADD COLUMN CutMode INTEGER NOT NULL DEFAULT 1;",
            "DrawerPulseOnMs" => "ALTER TABLE PrinterSettings ADD COLUMN DrawerPulseOnMs INTEGER NOT NULL DEFAULT 120;",
            "DrawerPulseOffMs" => "ALTER TABLE PrinterSettings ADD COLUMN DrawerPulseOffMs INTEGER NOT NULL DEFAULT 240;",
            _ => throw new InvalidOperationException($"Unsupported column '{columnName}'.")
        };

        db.Database.ExecuteSqlRaw(sql);
        columns.Add(columnName);
    }
}
