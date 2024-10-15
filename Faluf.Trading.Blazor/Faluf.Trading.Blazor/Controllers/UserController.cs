using Microsoft.AspNetCore.Mvc;

namespace Faluf.Trading.Blazor.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class UserController(IUserService userService) : ControllerBase
{
	[HttpPost("Register")]
	public async Task<ActionResult<Result<User>>> RegisterAsync(RegisterInputModel registerInputModel, CancellationToken cancellationToken)
	{
		Result<User> result = await userService.RegisterAsync(registerInputModel, cancellationToken);

		return StatusCode((int)result.StatusCode, result);
	}
}