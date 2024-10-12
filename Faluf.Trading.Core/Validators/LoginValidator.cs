namespace Faluf.Trading.Core.Validators;

public sealed class LoginValidator : AbstractValidator<LoginInputModel>
{
	public LoginValidator(IStringLocalizer<LoginValidator> stringLocalizer)
	{
		RuleFor(x => x.Email)
			.NotEmpty()
			.WithMessage(stringLocalizer["EmailRequired"]);
		RuleFor(x => x.Email)
			.EmailAddress()
			.WithMessage(stringLocalizer["EmailInvalid"]);
		RuleFor(x => x.Password)
			.NotEmpty()
			.WithMessage(stringLocalizer["PasswordRequired"]);
	}
}