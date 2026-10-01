using EstoqueFacil.Application.Dtos;
using FluentValidation;

namespace EstoqueFacil.Application.Validators
{
    public class UpdateStockMovementDtoValidator : AbstractValidator<UpdateStockMovementDto>
    {
        public UpdateStockMovementDtoValidator()
        {
            RuleFor(m => m.Quantity)
                .GreaterThan(0).WithMessage("A quantidade movimentada deve ser maior que zero.");

            RuleFor(m => m.Reason)
                .MaximumLength(300).WithMessage("O motivo deve ter no máximo 300 caracteres.");

            RuleFor(m => m.RequesterName)
                .MaximumLength(150).WithMessage("O nome do solicitante deve ter no máximo 150 caracteres.");

            RuleFor(m => m.Sector)
                .MaximumLength(100).WithMessage("O setor deve ter no máximo 100 caracteres.");
        }
    }
}
