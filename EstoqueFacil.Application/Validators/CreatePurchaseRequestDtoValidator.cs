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

            RuleFor(pr => pr.RequestNumber)
                .NotEmpty().WithMessage("O número da solicitação de compra é obrigatório.")
                .MaximumLength(50).WithMessage("O número da solicitação de compra deve ter no máximo 50 caracteres.");

            RuleFor(pr => pr.RequestDate)
                .NotEqual(default(DateTime)).WithMessage("Informe a data da solicitação.");
        }
    }
}
