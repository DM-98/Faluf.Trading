using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.Extensions.Localization;

namespace Faluf.Trading.Blazor.Client.Services;

public sealed class ClientAuthService(HttpClient httpClient, IStringLocalizer<ClientAuthService> stringLocalizer) : IAuthService
{
	public async Task<Result<TokenDTO>> LoginAsync(LoginInputModel loginInputModel, CancellationToken cancellationToken = default)
	{
		try
		{
			HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/Auth/Login", loginInputModel, cancellationToken);
			Result<TokenDTO> loginResult = await response.Content.ReadFromJsonAsync<Result<TokenDTO>>(cancellationToken) ?? Result.BadRequest<TokenDTO>(stringLocalizer["UnableToDeserialize"]);

			return loginResult;
		}
		catch (Exception ex)
		{
			return Result.InternalServerError<TokenDTO>(stringLocalizer["InternalServerError"], ex);
		}
	}

	public async Task<Result<IEnumerable<Claim>>> RefreshTokensAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			HttpResponseMessage response = await httpClient.GetAsync("api/Auth/RefreshTokens", cancellationToken);
			Result<IEnumerable<Claim>> refreshTokensResult = await response.Content.ReadFromJsonAsync<Result<IEnumerable<Claim>>>(cancellationToken) ?? Result.BadRequest<IEnumerable<Claim>>(stringLocalizer["UnableToDeserialize"]);

			return refreshTokensResult;
		}
		catch (Exception ex)
		{
			return Result.InternalServerError<IEnumerable<Claim>>(stringLocalizer["InternalServerError"], ex);
		}
	}

	public async Task<Result<IEnumerable<Claim>>> GetCurrentClaimsAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			HttpResponseMessage response = await httpClient.GetAsync("api/Auth/GetCurrentClaims", cancellationToken);
			Result<IEnumerable<Claim>> GetCurrentClaimsResult = await response.Content.ReadFromJsonAsync<Result<IEnumerable<Claim>>>(cancellationToken) ?? Result.BadRequest<IEnumerable<Claim>>(stringLocalizer["UnableToDeserialize"]);

			return GetCurrentClaimsResult;
		}
		catch (Exception ex)
		{
			return Result.InternalServerError<IEnumerable<Claim>>(stringLocalizer["InternalServerError"], ex);
		}
	}
}