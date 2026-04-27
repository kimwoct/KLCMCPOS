using KLCMC.Pos.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace KLCMC.Pos.App;

internal sealed class SimpleDbContextFactory : IDbContextFactory<PosDbContext>
{
    private readonly DbContextOptions<PosDbContext> _options;

    public SimpleDbContextFactory(DbContextOptions<PosDbContext> options)
    {
        _options = options;
    }

    public PosDbContext CreateDbContext() => new(_options);
}
