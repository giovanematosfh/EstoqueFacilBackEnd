using EstoqueFacil.Application.Dtos;
using FluentValidation;

namespace EstoqueFacil.Application.Validators
{
    public class CreatePurchaseRequestDtoValidator : AbstractValidator<CreatePurchaseRequestDto>
    {
        public CreatePurchaseRequestDtoValidator()
        {
            RuleFor(pr => pr.ProductId)
                .GreaterThan(0).WithMessage("Informe um produto válido.");

            RuleFor(pr => pr.BranchId)
                .GreaterThan(0).WithMessage("Informe uma filial válida.");

            RuleFor(pr => pr.Number)
                .NotEmpty().WithMessage("Informe o número da solicitação de compra.")
                .MaximumLength(50).WithMessage("O número da solicitação de compra deve ter no máximo 50 caracteres.");

            RuleFor(pr => pr.Quantity)
                .GreaterThan(0).WithMessage("A quantidade solicitada deve ser maior que zero.");

            RuleFor(pr => pr.RequestDate)
                .NotEmpty().WithMessage("Informe a data da solicitação de compra.");

            RuleFor(pr => pr.RequesterName)
                .MaximumLength(150).WithMessage("O nome do solicitante deve ter no máximo 150 caracteres.");

            RuleFor(pr => pr.Notes)
                .MaximumLength(500).WithMessage("A observação deve ter no máximo 500 caracteres.");
        }
    }
}
