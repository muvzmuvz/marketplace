using FluentValidation;
using Org.BouncyCastle.Crypto.Engines;


namespace marketplace_api.ModelsDto;

public class UserDtoValidator : AbstractValidator<UserDto>
{
    public UserDtoValidator()
    {
        RuleFor(user => user.Name).NotEmpty();
        RuleFor(user => user.Email).NotEmpty();
        RuleFor(user => user.Email).Length(9, 50);
        RuleFor(user => user.Name).Length(3, 40);
        RuleFor(user => user.HashPassword).Length(7, 30);
        RuleFor(user => user.Role)
            .Must(role => (int)role >= 1 && (int)role <= 2);
    }
}
