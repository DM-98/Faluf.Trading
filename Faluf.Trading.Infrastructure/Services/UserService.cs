using BCryptNext = BCrypt.Net.BCrypt;

namespace Faluf.Trading.Infrastructure.Services;

public sealed class UserService(IUserRepository userRepository, IStringLocalizer<UserService> stringLocalizer, ILogger<UserService> logger) 
	: IUserService
{
	public async Task<Result<User>> RegisterAsync(RegisterInputModel registerInputModel, CancellationToken cancellationToken = default)
	{
		try
		{
			User user = registerInputModel.Adapt<User>();

            user.HashedPassword = BCryptNext.HashPassword(registerInputModel.Password);
            user.Roles = ["User"];

            User createdUser = await userRepository.UpsertAsync(user, cancellationToken).ConfigureAwait(false);

            return Result.Created(createdUser);
        }
		catch (Exception ex)
		{
			logger.LogException(ex);
		
			return Result.InternalServerError<User>(stringLocalizer["InternalServerError"], ex);
		}
	}
}