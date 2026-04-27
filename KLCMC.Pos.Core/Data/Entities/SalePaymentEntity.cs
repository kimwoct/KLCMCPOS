namespace KLCMC.Pos.Core.Data.Entities;

public sealed class SalePaymentEntity
{
    public int Id { get; set; }
    public int SaleId { get; set; }
    public string Method { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal? TenderedAmount { get; set; }
    public decimal? ChangeAmount { get; set; }
}
