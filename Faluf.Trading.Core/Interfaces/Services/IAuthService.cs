using Refit;

namespace Faluf.Trading.Core.Interfaces.Services;

public interface IAuthService
{
    [Post("/api/Auth/Login")]
	Task<Result<TokenDTO>> LoginAsync([Body] LoginInputModel loginInputModel, CancellationToken cancellationToken = default);

    [Get("/api/Auth/ValidateRefreshToken/{refreshToken}")]
	Task<Result> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    [Post("/api/Auth/RefreshTokens")]
	Task<Result<TokenDTO>> RefreshTokensAsync([Body] TokenDTO tokenDTO, CancellationToken cancellationToken = default);

    [Get("/api/Auth/RevokeRefreshToken/{refreshToken}")]
	Task<Result> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    [Post("/api/Auth/RevokeUser/{userId}")]
	Task<Result> RevokeUserAsync(Guid userId, CancellationToken cancellationToken = default);
}