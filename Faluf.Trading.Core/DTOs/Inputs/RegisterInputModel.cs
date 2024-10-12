namespace Faluf.Trading.Core.DTOs.Inputs;

public sealed class RegisterInputModel
{
	public string Email { get; set; } = null!;

	public string FirstName { get; set; } = null!;

	public string LastName { get; set; } = null!;

	public string Password { get; set; } = null!;

	public string ConfirmPassword { get; set; } = null!;
}