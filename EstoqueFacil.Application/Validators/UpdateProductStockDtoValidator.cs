using EstoqueFacil.Application.Dtos;
using FluentValidation;

namespace EstoqueFacil.Application.Validators
{
    public class UpdateProductStockDtoValidator : AbstractValidator<UpdateProductStockDto>
    {
        public UpdateProductStockDtoValidator()
        {
            RuleFor(ps => ps.MinimumQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("O estoque mínimo não pode ser negativo.");
        }
    }
}
