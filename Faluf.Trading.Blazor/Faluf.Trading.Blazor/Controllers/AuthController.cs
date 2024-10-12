using Microsoft.AspNetCore.Mvc;

namespace Faluf.Trading.Blazor.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("Login")]
    public async Task<ActionResult<Result<TokenDTO>>> LoginAsync([FromBody] LoginInputModel loginInputModel, CancellationToken cancellationToken)
    {
        Result<TokenDTO> response = await authService.LoginAsync(loginInputModel, cancellationToken);

        if (!response.IsSuccess)
        {
            return response.StatusCode.HasValue ? StatusCode((int)response.StatusCode.Value, response) : BadRequest(response);
        }

        return Ok(response);
    }
}