using Microsoft.EntityFrameworkCore;

namespace Faluf.Trading.Infrastructure.Abstractions;

public abstract class BaseRepository<T, TDbContext>(IDbContextFactory<TDbContext> dbContextFactory) : IBaseRepository<T> where T : BaseEntity where TDbContext : DbContext
{
	protected IDbContextFactory<TDbContext> DbContextFactory { get; } = dbContextFactory; // https://www.benday.com/2024/07/18/how-to-fix-c-primary-constructor-warning-cs9107-parameter-is-captured-into-the-state-of-the-enclosing-type/

	public async Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default)
	{
		await using TDbContext context = await DbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

		await context.Set<T>().AddAsync(entity, cancellationToken).ConfigureAwait(false);
		await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

		return entity;
	}

	public async Task DeleteByIdAsync(Guid id, bool isSoftDelete = true, CancellationToken cancellationToken = default)
	{
		await using TDbContext context = await DbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

		if (isSoftDelete)
		{
			await context.Set<T>().Where(x => x.Id == id).ExecuteUpdateAsync(x => x.SetProperty(y => y.DeletedAtUTC, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
		}
		else
		{
			await context.Set<T>().Where(x => x.Id == id).ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
		}
	}

	public async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
	{
		await using TDbContext context = await DbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

		entity.UpdatedAtUTC = DateTime.UtcNow;

		await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

		return entity;
	}
}