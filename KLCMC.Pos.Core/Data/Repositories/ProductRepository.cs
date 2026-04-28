using KLCMC.Pos.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace KLCMC.Pos.Core.Data.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly IDbContextFactory<PosDbContext> _dbFactory;

    public ProductRepository(IDbContextFactory<PosDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public IReadOnlyList<ProductEntity> GetAll()
    {
        using var db = _dbFactory.CreateDbContext();
        return db.Products.AsNoTracking().Where(p => p.IsActive).OrderBy(p => p.Id).ToList();
    }

    public ProductEntity Add(string name, decimal defaultPrice)
    {
        using var db = _dbFactory.CreateDbContext();
        var product = new ProductEntity { Name = name, DefaultPrice = defaultPrice };
        db.Products.Add(product);
        db.SaveChanges();
        return product;
    }

    public bool Exists(string name)
    {
        using var db = _dbFactory.CreateDbContext();
        return db.Products.Any(p => EF.Functions.Like(p.Name, name));
    }

    public bool Update(int id, string name, decimal defaultPrice)
    {
        using var db = _dbFactory.CreateDbContext();
        var entity = db.Products.FirstOrDefault(p => p.Id == id);
        if (entity is null) return false;
        entity.Name = name;
        entity.DefaultPrice = defaultPrice;
        db.SaveChanges();
        return true;
    }

    public bool Delete(int id)
    {
        using var db = _dbFactory.CreateDbContext();
        var entity = db.Products.FirstOrDefault(p => p.Id == id);
        if (entity is null) return false;
        entity.IsActive = false;
        db.SaveChanges();
        return true;
    }
}
