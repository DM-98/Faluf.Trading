using Microsoft.EntityFrameworkCore;

namespace Faluf.Trading.Infrastructure.Repositories;

public sealed class UserRepository(IDbContextFactory<TradingDbContext> dbContextFactory) : BaseRepository<User, TradingDbContext>(dbContextFactory), IUserRepository
{
	public async Task<(IReadOnlyCollection<User> items, int recordCount)> GetUsersAsync(UserFilter filter, CancellationToken cancellationToken = default)
	{
		await using TradingDbContext context = await DbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
		IQueryable<User> query = context.Users;

		if (!string.IsNullOrWhiteSpace(filter.SearchString))
		{
			query = query.Where(x => x.Email.Contains(filter.SearchString) 
									|| x.FirstName.Contains(filter.SearchString) 
									|| x.LastName.Contains(filter.SearchString));
		}

		int recordCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

		query = query.Skip(filter.Page * filter.PageSize).Take(filter.PageSize);

		IReadOnlyCollection<User> items = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

		return (items, recordCount);
	}

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        await using TradingDbContext context = await DbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await context.Users.AsTracking().FirstOrDefaultAsync(x => x.Email == email, cancellationToken).ConfigureAwait(false);
    }

    public async Task<DateTimeOffset?> GetLockoutEndDateAsync(User user, CancellationToken cancellationToken = default)
    {
        await using TradingDbContext context = await DbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await context.Users.Where(x => x.Id == user.Id).Select(x => x.LockoutEndUTC).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<int> IncrementAccessFailedCountAsync(User user, CancellationToken cancellationToken = default)
    {
        await using TradingDbContext context = await DbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        int newAccessFailedCount = user.AccessFailedCount + 1;

        await context.Users.Where(x => x.Id == user.Id).ExecuteUpdateAsync(x => x.SetProperty(y => y.AccessFailedCount, newAccessFailedCount), cancellationToken).ConfigureAwait(false);

        return newAccessFailedCount;
    }
}