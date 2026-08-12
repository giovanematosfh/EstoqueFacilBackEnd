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

            RuleFor(ps => ps.PurchaseRequestNumber)
                .MaximumLength(50).WithMessage("O número da solicitação de compra deve ter no máximo 50 caracteres.");
        }
    }
}
