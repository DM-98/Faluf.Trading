using System.Net.Http.Json;
using Microsoft.Extensions.Localization;

namespace Faluf.Trading.Blazor.Client.Services;

public sealed class HttpClientAuthService(HttpClient httpClient, IStringLocalizer<HttpClientAuthService> stringLocalizer) : IAuthService
{
	public async Task<Result<TokenDTO>> LoginAsync(LoginInputModel loginInputModel, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/Auth/Login", loginInputModel, cancellationToken);

		if (!response.IsSuccessStatusCode)
		{
			Result<TokenDTO>? failedResult = await response.Content.ReadFromJsonAsync<Result<TokenDTO>>(cancellationToken);

			if (failedResult is not null)
			{
				return failedResult;
			}

			return Result.BadRequest<TokenDTO>(stringLocalizer["BadRequest"]);
		}

		TokenDTO? tokenDTO = await response.Content.ReadFromJsonAsync<TokenDTO>(cancellationToken);

		if (tokenDTO is null)
		{
			return Result.BadRequest<TokenDTO>(stringLocalizer["BadRequest"]);
		}

		return Result.Ok(tokenDTO);
	}

	public Task<Result<TokenDTO>> RefreshTokensAsync(TokenDTO tokenDTO, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public Task<Result> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public Task<Result> RevokeUserAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public Task<Result> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}
}