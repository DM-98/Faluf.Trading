using System.ComponentModel.DataAnnotations.Schema;

namespace Faluf.Trading.Core.Domain;

public sealed class AuthState : BaseEntity
{
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = default!;

    public string RefreshToken { get; set; } = default!;

    public DateTimeOffset RefreshTokentExpiryUTC { get; set; }

    public int AccessFailedCount { get; set; }

    public DateTimeOffset? LockoutEndUTC { get; set; }

	public required ClientType ClientType { get; set; }
}