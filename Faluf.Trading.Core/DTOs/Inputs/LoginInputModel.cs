namespace Faluf.Trading.Core.DTOs.Inputs;

public sealed class LoginInputModel
{
	public string Email { get; set; } = null!;

	public string Password { get; set; } = null!;

	public bool IsRememberMeChecked { get; set; }

	public required ClientType ClientType { get; init; }
}