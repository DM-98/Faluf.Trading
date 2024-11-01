namespace Faluf.Trading.Core.Abstractions.Interfaces;

public interface IBaseRepository<T> where T : BaseEntity
{
	Task<T> UpsertAsync(T entity, CancellationToken cancellationToken = default);
	
	Task DeleteByIdAsync(Guid id, bool isSoftDelete = true, CancellationToken cancellationToken = default);
}