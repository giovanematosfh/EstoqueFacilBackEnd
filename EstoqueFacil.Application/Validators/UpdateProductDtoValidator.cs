using EstoqueFacil.Application.Dtos;
using FluentValidation;

namespace EstoqueFacil.Application.Validators
{
    public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
    {
        public UpdateProductDtoValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("O nome do produto é obrigatório.")
                .MaximumLength(150).WithMessage("O nome do produto deve ter no máximo 150 caracteres.");

            RuleFor(p => p.Description)
                .MaximumLength(1000).WithMessage("A descrição deve ter no máximo 1000 caracteres.");

            RuleFor(p => p.Price)
                .GreaterThan(0).WithMessage("O preço deve ser maior que zero.");

            RuleFor(p => p.MinimumStockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("A quantidade mínima não pode ser negativa.");

            RuleFor(p => p.CategoryId)
                .GreaterThan(0).WithMessage("Informe uma categoria válida.");
        }
    }
}
