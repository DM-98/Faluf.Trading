using System.ComponentModel.DataAnnotations.Schema;

namespace Faluf.Trading.Core.Domain;

public sealed class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = default!;

    public string HashedToken { get; set; } = default!;

    public DateTimeOffset ExpiresAtUTC { get; set; }

    public DateTimeOffset? RevokedAtUTC { get; set; }
}