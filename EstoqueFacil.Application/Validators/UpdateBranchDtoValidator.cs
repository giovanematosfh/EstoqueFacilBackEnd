using EstoqueFacil.Application.Dtos;
using FluentValidation;

namespace EstoqueFacil.Application.Validators
{
    public class UpdateBranchDtoValidator : AbstractValidator<UpdateBranchDto>
    {
        public UpdateBranchDtoValidator()
        {
            RuleFor(b => b.Name)
                .NotEmpty().WithMessage("O nome da filial é obrigatório.")
                .MaximumLength(150).WithMessage("O nome da filial deve ter no máximo 150 caracteres.");

            RuleFor(b => b.Address)
                .MaximumLength(300).WithMessage("O endereço deve ter no máximo 300 caracteres.");
        }
    }
}
