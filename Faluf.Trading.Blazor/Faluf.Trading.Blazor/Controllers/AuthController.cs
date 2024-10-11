using Faluf.Trading.Core.DTOs.Inputs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Faluf.Trading.Blazor.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<Result>> LoginAsync(Core.DTOs.Inputs.LoginInputModel loginInputModel, CancellationToken cancellationToken)
    {
        Result<TokenDTO> response = await authService.LoginAsync(loginInputModel, cancellationToken);

        if (!response.IsSuccess)
        {
            return response.StatusCode.HasValue ? StatusCode((int)response.StatusCode.Value, response) : BadRequest(response);
        }

        return Ok(response);
    }
}