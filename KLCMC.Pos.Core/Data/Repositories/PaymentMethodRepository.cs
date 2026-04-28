using KLCMC.Pos.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace KLCMC.Pos.Core.Data.Repositories;

public sealed class PaymentMethodRepository : IPaymentMethodRepository
{
    private readonly IDbContextFactory<PosDbContext> _dbFactory;

    public PaymentMethodRepository(IDbContextFactory<PosDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public IReadOnlyList<PaymentMethodEntity> GetAll()
    {
        using var db = _dbFactory.CreateDbContext();
        return db.PaymentMethods.AsNoTracking()
            .Where(m => m.IsActive)
            .OrderBy(m => m.SortOrder)
            .ThenBy(m => m.Id)
            .ToList();
    }

    public PaymentMethodEntity Add(string name)
    {
        using var db = _dbFactory.CreateDbContext();
        var maxOrder = db.PaymentMethods.Any() ? db.PaymentMethods.Max(m => m.SortOrder) : -1;
        var entity = new PaymentMethodEntity { Name = name, SortOrder = maxOrder + 1 };
        db.PaymentMethods.Add(entity);
        db.SaveChanges();
        return entity;
    }

    public bool Update(int id, string name)
    {
        using var db = _dbFactory.CreateDbContext();
        var entity = db.PaymentMethods.FirstOrDefault(m => m.Id == id);
        if (entity is null) return false;
        entity.Name = name;
        db.SaveChanges();
        return true;
    }

    public bool Delete(int id)
    {
        using var db = _dbFactory.CreateDbContext();
        var entity = db.PaymentMethods.FirstOrDefault(m => m.Id == id);
        if (entity is null) return false;
        entity.IsActive = false;
        db.SaveChanges();
        return true;
    }
}
