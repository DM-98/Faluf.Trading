using Microsoft.EntityFrameworkCore;

namespace Faluf.Trading.Infrastructure.Repositories;

public sealed class RefreshTokenRepository(IDbContextFactory<TradingDbContext> dbContextFactory) : BaseRepository<RefreshToken, TradingDbContext>(dbContextFactory), IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        await using TradingDbContext context = await DbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IEnumerable<RefreshToken>> GetRefreshTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using TradingDbContext context = await DbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await context.RefreshTokens.Where(x => x.UserId == userId).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> IsRefreshTokenBlacklistedAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        await using TradingDbContext context = await DbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        return await context.RefreshTokens.AnyAsync(x => x.Token == refreshToken && x.RevokedAtUTC != null, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateRangeAsync(IEnumerable<RefreshToken> refreshTokens, CancellationToken cancellationToken)
    {
        await using TradingDbContext context = await DbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

        foreach (RefreshToken refreshToken in refreshTokens)
        {
            refreshToken.UpdatedAtUTC = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}