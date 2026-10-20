using EstoqueFacil.Application.Dtos;
using FluentValidation;

namespace EstoqueFacil.Application.Validators
{
    public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordDtoValidator()
        {
            RuleFor(p => p.CurrentPassword)
                .NotEmpty().WithMessage("Informe a senha atual.");

            RuleFor(p => p.NewPassword)
                .NotEmpty().WithMessage("Informe a nova senha.")
                .MinimumLength(6).WithMessage("A senha deve ter no mínimo 6 caracteres.")
                .NotEqual(p => p.CurrentPassword).WithMessage("A nova senha deve ser diferente da atual.");
        }
    }
}
