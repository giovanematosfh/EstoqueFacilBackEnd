using EstoqueFacil.Application.Constants;
using EstoqueFacil.Application.Dtos;
using FluentValidation;

namespace EstoqueFacil.Application.Validators
{
    public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
    {
        public CreateUserDtoValidator()
        {
            RuleFor(u => u.FullName)
                .NotEmpty().WithMessage("O nome é obrigatório.")
                .MaximumLength(150).WithMessage("O nome deve ter no máximo 150 caracteres.");

            RuleFor(u => u.Email)
                .NotEmpty().WithMessage("O e-mail é obrigatório.")
                .EmailAddress().WithMessage("Informe um e-mail válido.");

            RuleFor(u => u.Password)
                .NotEmpty().WithMessage("A senha é obrigatória.")
                .MinimumLength(6).WithMessage("A senha deve ter no mínimo 6 caracteres.");

            RuleFor(u => u.Role)
                .Must(r => r == Roles.Admin || r == Roles.User).WithMessage("Papel inválido. Use 'Admin' ou 'User'.");
        }
    }
}
