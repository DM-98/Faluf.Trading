namespace Faluf.Trading.Core.Interfaces.Repositories;

public interface IUserRepository : IBaseRepository<User>
{
	Task<(IReadOnlyCollection<User> Items, int RecordCount)> GetUsersAsync(UserFilter filter, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}