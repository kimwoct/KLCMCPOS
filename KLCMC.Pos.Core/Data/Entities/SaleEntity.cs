namespace KLCMC.Pos.Core.Data.Entities;

public sealed class SaleEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public decimal Total { get; set; }
    public List<SaleLineEntity> Lines { get; set; } = new();
    public List<SalePaymentEntity> Payments { get; set; } = new();
}
