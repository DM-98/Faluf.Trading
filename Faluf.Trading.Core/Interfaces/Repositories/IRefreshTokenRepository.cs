namespace Faluf.Trading.Core.Interfaces.Repositories;

public interface IRefreshTokenRepository : IBaseRepository<RefreshToken>
{
    Task<RefreshToken?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task<IEnumerable<RefreshToken>> GetRefreshTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<bool> IsRefreshTokenBlacklistedAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task UpdateRangeAsync(IEnumerable<RefreshToken> refreshTokens, CancellationToken cancellationToken = default);
}