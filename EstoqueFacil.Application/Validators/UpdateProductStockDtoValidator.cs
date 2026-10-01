using EstoqueFacil.Application.Dtos;
using FluentValidation;

namespace EstoqueFacil.Application.Validators
{
    public class UpdateProductStockDtoValidator : AbstractValidator<UpdateProductStockDto>
    {
        public UpdateProductStockDtoValidator()
        {
            RuleFor(ps => ps.Quantity)
                .GreaterThanOrEqualTo(0).WithMessage("A quantidade em estoque não pode ser negativa.");

            RuleFor(ps => ps.MinimumQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("O estoque de segurança não pode ser negativo.");
        }
    }
}
