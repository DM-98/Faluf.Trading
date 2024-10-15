using System.Net.Http.Json;
using Microsoft.Extensions.Localization;

namespace Faluf.Trading.Blazor.Client.Services;

public sealed class HttpClientUserService(HttpClient httpClient, IStringLocalizer<HttpClientAuthService> stringLocalizer) : IUserService
{
	public async Task<Result<User>> RegisterAsync(RegisterInputModel registerInputModel, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/User/Register", registerInputModel, cancellationToken);

		if (!response.IsSuccessStatusCode)
		{
			Result<User>? failedResult = await response.Content.ReadFromJsonAsync<Result<User>>(cancellationToken);

			if (failedResult is not null)
			{
				return failedResult;
			}

			return Result.BadRequest<User>(stringLocalizer["BadRequest"]);
		}

		User? user = await response.Content.ReadFromJsonAsync<User>(cancellationToken);

		if (user is null)
		{
			return Result.BadRequest<User>(stringLocalizer["BadRequest"]);
		}

		return Result.Ok(user);
	}
}