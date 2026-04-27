using KLCMC.Pos.Core.Data.Entities;

namespace KLCMC.Pos.Core.Data;

public static class DatabaseInitializer
{
    private static readonly (string Name, decimal Price)[] DefaultProducts =
    {
        ("Americano", 6.50m),
        ("Latte", 8.90m),
        ("Cappuccino", 8.90m),
        ("Sandwich", 12.00m),
        ("Muffin", 5.00m),
        ("Mineral Water", 2.00m)
    };

    public static void EnsureReady(PosDbContext db)
    {
        db.Database.EnsureCreated();

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
            db.PrinterSettings.Add(new PrinterSettingEntity { Id = 1 });
            db.SaveChanges();
        }
    }
}
