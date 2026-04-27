namespace KLCMC.Pos.Core.Data.Entities;

public sealed class SaleLineEntity
{
    public int Id { get; set; }
    public int SaleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string Remark { get; set; } = string.Empty;
    public decimal LineTotal { get; set; }
}
