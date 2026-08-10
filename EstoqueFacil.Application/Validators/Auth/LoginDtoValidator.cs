using EstoqueFacil.Application.Dtos.Auth;
using FluentValidation;

namespace EstoqueFacil.Application.Validators.Auth
{
    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(l => l.Email)
                .NotEmpty().WithMessage("O e-mail é obrigatório.")
                .EmailAddress().WithMessage("Informe um e-mail válido.");

            RuleFor(l => l.Password)
                .NotEmpty().WithMessage("A senha é obrigatória.");
        }
    }
}
