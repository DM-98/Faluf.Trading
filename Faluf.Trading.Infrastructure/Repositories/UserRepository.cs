using Microsoft.EntityFrameworkCore;

namespace Faluf.Trading.Infrastructure.Repositories;

public sealed class UserRepository(IDbContextFactory<TradingDbContext> dbContextFactory) 
	: BaseRepository<User, TradingDbContext>(dbContextFactory), IUserRepository
{
	public async Task<(IReadOnlyCollection<User> Items, int RecordCount)> GetUsersAsync(UserFilter filter, CancellationToken cancellationToken = default)
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

		query = query.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize);

		IReadOnlyCollection<User> items = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

		return (items, recordCount);
	}

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        await using TradingDbContext context = await DbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await context.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken).ConfigureAwait(false);
    }
}