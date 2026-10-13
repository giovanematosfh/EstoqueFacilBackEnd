using EstoqueFacil.Application.Dtos;
using FluentValidation;

namespace EstoqueFacil.Application.Validators
{
    public class SavePlanDtoValidator : AbstractValidator<SavePlanDto>
    {
        public SavePlanDtoValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("O nome do plano é obrigatório.")
                .MaximumLength(100).WithMessage("O nome do plano deve ter no máximo 100 caracteres.");

            RuleFor(p => p.Price)
                .GreaterThanOrEqualTo(0).WithMessage("O preço não pode ser negativo.");

            RuleFor(p => p.MaxUsers)
                .GreaterThan(0).When(p => p.MaxUsers.HasValue).WithMessage("O limite de usuários deve ser maior que zero.");

            RuleFor(p => p.MaxBranches)
                .GreaterThan(0).When(p => p.MaxBranches.HasValue).WithMessage("O limite de filiais deve ser maior que zero.");

            RuleFor(p => p.MaxProducts)
                .GreaterThan(0).When(p => p.MaxProducts.HasValue).WithMessage("O limite de produtos deve ser maior que zero.");
        }
    }
}
