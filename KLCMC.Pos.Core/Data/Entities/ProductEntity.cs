namespace KLCMC.Pos.Core.Data.Entities;

public sealed class ProductEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal DefaultPrice { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
