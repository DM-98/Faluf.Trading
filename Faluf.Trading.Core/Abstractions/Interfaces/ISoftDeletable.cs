namespace Faluf.Trading.Core.Abstractions.Interfaces;

public interface ISoftDeletable
{
	DateTime? DeletedAtUTC { get; set; }
}