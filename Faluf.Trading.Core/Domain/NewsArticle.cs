namespace Faluf.Trading.Core.Domain;

public sealed class NewsArticle : BaseEntity
{
	public string Heading { get; set; } = null!;

	public string? Subheading { get; set; }

	public string BodyText { get; set; } = null!;

	public DateTimeOffset PublishedAt { get; set; }

	public string? AuthorName { get; set; }
}