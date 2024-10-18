using System.Net.Http.Json;
using BitzArt.Blazor.Cookies;
using Microsoft.Extensions.Localization;

namespace Faluf.Trading.Blazor.Client.Services;

public sealed class ClientAuthService(ICookieService cookieService, HttpClient httpClient, IStringLocalizer<ClientAuthService> stringLocalizer) : IAuthService
{
	public async Task<Result<TokenDTO>> LoginAsync(LoginInputModel loginInputModel, CancellationToken cancellationToken = default)
	{
		try
		{
			HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/Auth/Login", loginInputModel, cancellationToken);
			Result<TokenDTO> loginResult = await response.Content.ReadFromJsonAsync<Result<TokenDTO>>(cancellationToken) ?? Result.BadRequest<TokenDTO>(stringLocalizer["UnableToDeserialize"]);

			if (loginResult.IsSuccess)
			{
				await cookieService.SetAsync("accessToken", loginResult.Content.AccessToken, loginInputModel.IsRememberMeChecked ? DateTime.Now.AddYears(1) : null, cancellationToken);
				await cookieService.SetAsync("rememberMe", loginInputModel.IsRememberMeChecked.ToString(), DateTime.Now.AddYears(1), cancellationToken);
			}

			return loginResult;
		}
		catch (Exception ex)
		{
			return Result.InternalServerError<TokenDTO>(stringLocalizer["InternalServerError"], ex);
		}
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