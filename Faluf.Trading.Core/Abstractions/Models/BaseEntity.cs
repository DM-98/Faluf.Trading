using System.ComponentModel.DataAnnotations;

namespace Faluf.Trading.Core.Abstractions.Models;

public abstract class BaseEntity : ISoftDeletable
{
	[Key]
	public Guid Id { get; set; }

	public DateTime CreatedAtUTC { get; set; } = DateTime.UtcNow;

	public DateTime? UpdatedAtUTC { get; set; }

	public DateTime? DeletedAtUTC { get; set; }
}