using KLCMC.Pos.Core.Data.Entities;

namespace KLCMC.Pos.Core.Data.Repositories;

public interface IProductRepository
{
    IReadOnlyList<ProductEntity> GetAll();
    ProductEntity Add(string name, decimal defaultPrice);
    bool Exists(string name);
}
