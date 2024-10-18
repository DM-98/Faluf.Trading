namespace Faluf.Trading.Core.Domain;

public sealed class User : BaseEntity
{
	public string FirstName { get; set; } = null!;

	public string LastName { get; set; } = null!;

	public string Email { get; set; } = null!;

	public string HashedPassword { get; set; } = null!;

    public List<string> Roles { get; set; } = [];

	public ICollection<AuthState> AuthStates { get; set; } = [];
}