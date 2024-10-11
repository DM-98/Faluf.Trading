namespace Faluf.Trading.Core.Interfaces.Repositories;

public interface INewsArticleRepository : IBaseRepository<NewsArticle>
{
	Task<(IReadOnlyCollection<NewsArticle> Items, int RecordCount)> GetNewsArticlesAsync(NewsArticleFilter newsArticleFilter, CancellationToken cancellationToken = default);

	Task<NewsArticle?> GetNewsArticleByIdAsync(Guid id, CancellationToken cancellationToken = default);
}