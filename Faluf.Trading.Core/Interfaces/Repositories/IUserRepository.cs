namespace Faluf.Trading.Core.Interfaces.Repositories;

public interface IUserRepository : IBaseRepository<User>
{
	Task<(IReadOnlyCollection<User> items, int recordCount)> GetUsersAsync(UserFilter filter, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<DateTimeOffset?> GetLockoutEndDateAsync(User user, CancellationToken cancellationToken = default);
}