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
}