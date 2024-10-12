namespace Faluf.Trading.Core.Interfaces.Services;

public interface IAuthService
{
	Task<Result<TokenDTO>> LoginAsync(LoginInputModel loginInputModel, CancellationToken cancellationToken = default);

	Task<Result> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

	Task<Result<TokenDTO>> RefreshTokensAsync(TokenDTO tokenDTO, CancellationToken cancellationToken = default);

	Task<Result> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

	Task<Result> RevokeUserAsync(Guid userId, CancellationToken cancellationToken = default);
}