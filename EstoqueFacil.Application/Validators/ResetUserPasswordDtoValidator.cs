using EstoqueFacil.Application.Dtos;
using FluentValidation;

namespace EstoqueFacil.Application.Validators
{
    public class ResetUserPasswordDtoValidator : AbstractValidator<ResetUserPasswordDto>
    {
        public ResetUserPasswordDtoValidator()
        {
            RuleFor(p => p.NewPassword)
                .NotEmpty().WithMessage("Informe a nova senha.")
                .MinimumLength(6).WithMessage("A senha deve ter no mínimo 6 caracteres.");
        }
    }
}
