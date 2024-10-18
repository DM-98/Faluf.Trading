using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Faluf.Trading.Infrastructure.Contexts;

public sealed class TradingDbContext(DbContextOptions<TradingDbContext> options) : DbContext(options), IDataProtectionKeyContext
{
	public DbSet<User> Users => Set<User>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
		{
			if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
			{
				entityType.AddSoftDeleteQueryFilter();
			}
		}
	}
}