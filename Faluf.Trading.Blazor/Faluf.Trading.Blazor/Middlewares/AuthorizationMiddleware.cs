using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BitzArt.Blazor.Cookies;

namespace Faluf.Trading.Blazor.Middlewares;

public sealed class AuthorizationMiddleware(RequestDelegate next)
{

	public async Task InvokeAsync(HttpContext httpContext, ICookieService cookieService)
	{
		Cookie? accessTokenCookie = await cookieService.GetAsync("accessToken");

		if (accessTokenCookie is not null)
		{
			IEnumerable<Claim> claims = new JwtSecurityTokenHandler().ReadJwtToken(accessTokenCookie.Value).Claims;

			httpContext.User = new(new ClaimsIdentity(claims, "jwt"));
		}

		await next(httpContext);
	}
}

public static class JWTAuthorizationMiddlewareExtensions
{
	public static IApplicationBuilder UseJWTAuthorizationMiddleware(this IApplicationBuilder builder)
	{
		return builder.UseMiddleware<AuthorizationMiddleware>();
	}
}