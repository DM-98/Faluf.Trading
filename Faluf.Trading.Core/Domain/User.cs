namespace Faluf.Trading.Core.Domain;

public sealed class User : BaseEntity
{
	public string FirstName { get; init; } = null!;

	public string LastName { get; init; } = null!;

	public string Email { get; init; } = null!;

	public string HashedPassword { get; set; } = null!;

    public List<string> Roles { get; set; } = [];

	public ICollection<AuthState> AuthStates { get; init; } = [];
}