using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BitzArt.Blazor.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace Faluf.Trading.Blazor.Services;

public sealed class BlazorAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
	public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
	{
		ICookieService cookieService = context.RequestServices.GetRequiredService<ICookieService>();

		Cookie? accessTokenCookie = await cookieService.GetAsync("accessToken");

		if (accessTokenCookie is not null)
		{
			IEnumerable<Claim> claims = new JwtSecurityTokenHandler().ReadJwtToken(accessTokenCookie.Value).Claims;

			context.User = new(new ClaimsIdentity(claims, "jwt"));
		}

		await next(context);
	}
}