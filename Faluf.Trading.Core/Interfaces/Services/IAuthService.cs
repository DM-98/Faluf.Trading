using System.Security.Claims;

namespace Faluf.Trading.Core.Interfaces.Services;

public interface IAuthService
{
	Task<Result<TokenDTO>> LoginAsync(LoginInputModel loginInputModel, CancellationToken cancellationToken = default);

	Task<Result<IEnumerable<Claim>>> RefreshTokensAsync(CancellationToken cancellationToken = default);

	Task<Result<IEnumerable<Claim>>> GetCurrentClaimsAsync(CancellationToken cancellationToken = default);
}