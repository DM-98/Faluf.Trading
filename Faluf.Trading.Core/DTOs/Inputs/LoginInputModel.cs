namespace Faluf.Trading.Core.DTOs.Inputs;

public class LoginInputModel
{
	public string Email { get; set; } = null!;

	public string Password { get; set; } = null!;

    public required ClientType ClientType { get; set; }
}