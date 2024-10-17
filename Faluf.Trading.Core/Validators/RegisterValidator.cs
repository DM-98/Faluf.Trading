namespace Faluf.Trading.Core.Validators;

public sealed class RegisterValidator : AbstractValidator<RegisterInputModel>
{
	public RegisterValidator(IStringLocalizer<RegisterValidator> stringLocalizer)
	{
		RuleFor(x => x.FirstName)
			.NotEmpty()
			.WithMessage(stringLocalizer["FirstNameRequired"]);
		RuleFor(x => x.LastName)
			.NotEmpty()
			.WithMessage(stringLocalizer["LastNameRequired"]);
		RuleFor(x => x.Email)
			.NotEmpty()
			.WithMessage(stringLocalizer["EmailRequired"]);
		RuleFor(x => x.Email)
			.EmailAddress()
			.WithMessage(stringLocalizer["EmailInvalid"]);
		RuleFor(x => x.Password)
			.NotEmpty()
			.WithMessage(stringLocalizer["PasswordRequired"]);
		RuleFor(x => x.ConfirmPassword)
			.NotEmpty()
			.WithMessage(stringLocalizer["ConfirmPasswordRequired"])
			.Equal(x => x.Password)
			.WithMessage(stringLocalizer["PasswordsDoNotMatch"]);
	}
}