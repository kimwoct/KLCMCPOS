using KLCMC.Pos.Core.Data.Entities;

namespace KLCMC.Pos.Core.Data.Repositories;

public interface IPaymentMethodRepository
{
    IReadOnlyList<PaymentMethodEntity> GetAll();
    PaymentMethodEntity Add(string name);
    bool Update(int id, string name);
    bool Delete(int id);
}
