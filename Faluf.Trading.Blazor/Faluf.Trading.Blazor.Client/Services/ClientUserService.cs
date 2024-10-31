using System.Net.Http.Json;
using Microsoft.Extensions.Localization;

namespace Faluf.Trading.Blazor.Client.Services;

public sealed class ClientUserService(HttpClient httpClient, IStringLocalizer<ClientAuthService> stringLocalizer) : IUserService
{
	public async Task<Result> OkTest(CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await httpClient.GetAsync("api/User/Test", cancellationToken);

		return response.IsSuccessStatusCode ? Result.Ok() : Result.BadRequest(stringLocalizer["UnableToGetOkTest"]);
	}

	public async Task<Result<User>> RegisterAsync(RegisterInputModel registerInputModel, CancellationToken cancellationToken = default)
	{
		HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/User/Register", registerInputModel, cancellationToken);
		Result<User> userResult = await response.Content.ReadFromJsonAsync<Result<User>>(cancellationToken) ?? Result.BadRequest<User>(stringLocalizer["UnableToDeserialize"]);

		return userResult;
	}
}