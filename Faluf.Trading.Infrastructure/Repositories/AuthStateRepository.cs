using Faluf.Trading.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace Faluf.Trading.Infrastructure.Repositories;

public sealed class AuthStateRepository(IDbContextFactory<TradingDbContext> dbContextFactory) : BaseRepository<AuthState, TradingDbContext>(dbContextFactory), IAuthStateRepository
{
    public async Task<AuthState?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        await using TradingDbContext context = await DbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await context.AuthStates.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken, cancellationToken).ConfigureAwait(false);
    }

	public async Task<AuthState?> GetByUserIdAndClientTypeAsync(Guid id, ClientType clientType, CancellationToken cancellationToken = default)
	{
		await using TradingDbContext context = await DbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

		return await context.AuthStates.FirstOrDefaultAsync(x => x.UserId == id && x.ClientType == clientType, cancellationToken).ConfigureAwait(false);
	}

	public async Task<IEnumerable<AuthState>> GetRefreshTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using TradingDbContext context = await DbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await context.AuthStates.Where(x => x.UserId == userId).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateRangeAsync(IEnumerable<AuthState> refreshTokens, CancellationToken cancellationToken)
    {
        await using TradingDbContext context = await DbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        foreach (AuthState refreshToken in refreshTokens)
        {
            refreshToken.UpdatedAtUTC = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}