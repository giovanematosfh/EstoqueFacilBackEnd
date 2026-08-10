using EstoqueFacil.Application.Dtos;
using FluentValidation;

namespace EstoqueFacil.Application.Validators
{
    public class UpdateCategoryDtoValidator : AbstractValidator<UpdateCategoryDto>
    {
        public UpdateCategoryDtoValidator()
        {
            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("O nome da categoria é obrigatório.")
                .MaximumLength(100).WithMessage("O nome da categoria deve ter no máximo 100 caracteres.");

            RuleFor(c => c.Description)
                .MaximumLength(500).WithMessage("A descrição deve ter no máximo 500 caracteres.");
        }
    }
}
