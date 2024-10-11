using Microsoft.EntityFrameworkCore;

namespace Faluf.Trading.Infrastructure.Repositories;

public sealed class NewsArticleRepository(IDbContextFactory<TradingDbContext> dbContextFactory) : BaseRepository<NewsArticle, TradingDbContext>(dbContextFactory), INewsArticleRepository
{
	public async Task<NewsArticle?> GetNewsArticleByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		await using TradingDbContext context = await DbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);

		return await context.NewsArticles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken).ConfigureAwait(false);
	}

	public async Task<(IReadOnlyCollection<NewsArticle> Items, int RecordCount)> GetNewsArticlesAsync(NewsArticleFilter filter, CancellationToken cancellationToken = default)
	{
		await using TradingDbContext context = await DbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
		IQueryable<NewsArticle> query;

		if (!string.IsNullOrWhiteSpace(filter.SearchString))
		{
			query = context.NewsArticles.FromSql($"SELECT * FROM Articles WHERE CONTAINS(BodyText, {filter.SearchString}) OR CONTAINS(Heading, {filter.SearchString}) OR CONTAINS(Subheading, {filter.SearchString}) OR CONTAINS(AuthorName, {filter.SearchString})").OrderByDescending(x => x.PublishedAt);
		}
		else
		{
			query = context.NewsArticles.OrderByDescending(x => x.PublishedAt);
		}

		int recordCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

		query = query.Skip(filter.Page * filter.PageSize).Take(filter.PageSize);

		IReadOnlyCollection<NewsArticle> items = await query.ToListAsync(cancellationToken).ConfigureAwait(false);

		return (items, recordCount);
	}
}