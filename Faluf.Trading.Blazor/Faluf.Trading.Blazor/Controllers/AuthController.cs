using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Faluf.Trading.Blazor.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
	[HttpPost("Login")]
	public async Task<ActionResult<Result<TokenDTO>>> LoginAsync(LoginInputModel loginInputModel, CancellationToken cancellationToken)
	{
		Result<TokenDTO> result = await authService.LoginAsync(loginInputModel, cancellationToken);

		return StatusCode((int)result.StatusCode, result);
	}

	[HttpPost("RefreshTokens")]
	public async Task<ActionResult<Result<TokenDTO>>> RefreshTokensAsync(CancellationToken cancellationToken)
	{
		Result<IEnumerable<Claim>> result = await authService.RefreshTokensAsync(cancellationToken);

		return StatusCode((int)result.StatusCode, result);
	}

	[HttpGet("GetCurrentClaims")]
	[Authorize]
	public async Task<ActionResult<Result<IEnumerable<Claim>>>> GetCurrentClaimsAsync(CancellationToken cancellationToken)
	{
		Result<IEnumerable<Claim>> result = await authService.GetCurrentClaimsAsync(cancellationToken);

		return StatusCode((int)result.StatusCode, result);
	}

	[HttpPost("Logout")]
	public ActionResult Logout()
	{
		HttpContext.Response.Cookies.Delete("accessToken");
		HttpContext.Response.Cookies.Delete("rememberMe");

		return LocalRedirect("/");
	}
}