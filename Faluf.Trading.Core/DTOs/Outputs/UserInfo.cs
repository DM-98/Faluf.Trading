namespace Faluf.Trading.Core.DTOs.Outputs;

public sealed class UserInfo
{
    public string UserId { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;
}