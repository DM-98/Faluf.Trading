namespace Faluf.Trading.Core.DTOs.Inputs;

public sealed class RegisterInputModel : LoginInputModel
{
	public string FirstName { get; set; } = null!;

	public string LastName { get; set; } = null!;

	public string ConfirmPassword { get; set; } = null!;
}