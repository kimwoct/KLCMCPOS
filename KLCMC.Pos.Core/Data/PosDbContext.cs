using KLCMC.Pos.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace KLCMC.Pos.Core.Data;

public sealed class PosDbContext : DbContext
{
    public PosDbContext(DbContextOptions<PosDbContext> options) : base(options)
    {
    }

    public DbSet<ProductEntity> Products => Set<ProductEntity>();
    public DbSet<PaymentMethodEntity> PaymentMethods => Set<PaymentMethodEntity>();
    public DbSet<SaleEntity> Sales => Set<SaleEntity>();
    public DbSet<SaleLineEntity> SaleLines => Set<SaleLineEntity>();
    public DbSet<SalePaymentEntity> SalePayments => Set<SalePaymentEntity>();
    public DbSet<PrinterSettingEntity> PrinterSettings => Set<PrinterSettingEntity>();
    public DbSet<UiAppearanceSettingEntity> UiAppearanceSettings => Set<UiAppearanceSettingEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProductEntity>(b =>
        {
            b.Property(p => p.Name).IsRequired().HasMaxLength(120);
            b.HasIndex(p => p.Name).IsUnique();
            b.Property(p => p.DefaultPrice).HasColumnType("TEXT");
        });

        modelBuilder.Entity<SaleEntity>(b =>
        {
            b.Property(s => s.Total).HasColumnType("TEXT");
            b.HasIndex(s => s.CreatedAt);
            b.HasMany(s => s.Lines)
                .WithOne()
                .HasForeignKey(l => l.SaleId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasMany(s => s.Payments)
                .WithOne()
                .HasForeignKey(p => p.SaleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SalePaymentEntity>(b =>
        {
            b.Property(p => p.Method).IsRequired().HasMaxLength(40);
            b.Property(p => p.Amount).HasColumnType("TEXT");
            b.Property(p => p.TenderedAmount).HasColumnType("TEXT");
            b.Property(p => p.ChangeAmount).HasColumnType("TEXT");
            b.HasIndex(p => p.SaleId);
        });

        modelBuilder.Entity<SaleLineEntity>(b =>
        {
            b.Property(l => l.Name).IsRequired().HasMaxLength(120);
            b.Property(l => l.UnitPrice).HasColumnType("TEXT");
            b.Property(l => l.LineTotal).HasColumnType("TEXT");
            b.Property(l => l.Remark).HasMaxLength(500);
        });

        modelBuilder.Entity<PrinterSettingEntity>(b =>
        {
            b.Property(p => p.Endpoint).HasMaxLength(120);
            b.Property(p => p.CodePage).HasMaxLength(40);
        });

        modelBuilder.Entity<UiAppearanceSettingEntity>(b =>
        {
            b.Property(p => p.PrimaryTextColor).IsRequired().HasMaxLength(7);
            b.Property(p => p.SecondaryTextColor).IsRequired().HasMaxLength(7);
            b.Property(p => p.BackgroundColor).IsRequired().HasMaxLength(7);
            b.Property(p => p.AccentColor).IsRequired().HasMaxLength(7);
        });
    }
}
