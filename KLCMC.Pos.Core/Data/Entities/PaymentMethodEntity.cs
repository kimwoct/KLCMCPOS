namespace KLCMC.Pos.Core.Data.Entities;

public sealed class PaymentMethodEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
