using EstoqueFacil.Application.Dtos;
using FluentValidation;

namespace EstoqueFacil.Application.Validators
{
    public class CreateStockMovementDtoValidator : AbstractValidator<CreateStockMovementDto>
    {
        public CreateStockMovementDtoValidator()
        {
            RuleFor(m => m.ProductId)
                .GreaterThan(0).WithMessage("Informe um produto válido.");

            RuleFor(m => m.Type)
                .IsInEnum().WithMessage("Tipo de movimentação inválido.");

            RuleFor(m => m.Quantity)
                .GreaterThan(0).WithMessage("A quantidade movimentada deve ser maior que zero.");

            RuleFor(m => m.Reason)
                .MaximumLength(300).WithMessage("O motivo deve ter no máximo 300 caracteres.");
        }
    }
}
