using System.Net.Http.Json;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;

namespace Faluf.Trading.Blazor.Client.Services;

public sealed class ClientAuthService(IJSRuntime jsRuntime, HttpClient httpClient, IStringLocalizer<ClientAuthService> stringLocalizer) : IAuthService
{
	public async Task<Result<TokenDTO>> LoginAsync(LoginInputModel loginInputModel, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/Auth/Login", loginInputModel, cancellationToken);
		Result<TokenDTO>? loginResult = await response.Content.ReadFromJsonAsync<Result<TokenDTO>>(cancellationToken);

		if (loginResult is null or { IsSuccess: false })
		{
			return Result.BadRequest<TokenDTO>(stringLocalizer["BadRequest"]);
		}

		await jsRuntime.InvokeVoidAsync("localStorage.setItem", "accessToken", loginResult.Content.AccessToken);

		return loginResult;
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