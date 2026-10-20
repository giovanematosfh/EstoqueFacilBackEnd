using EstoqueFacil.Application.Dtos;
using FluentValidation;

namespace EstoqueFacil.Application.Validators
{
    public class CreatePurchaseRequestDtoValidator : AbstractValidator<CreatePurchaseRequestDto>
    {
        public CreatePurchaseRequestDtoValidator()
        {
            RuleFor(pr => pr.BranchId)
                .GreaterThan(0).WithMessage("Informe uma filial válida.");

            RuleFor(pr => pr.Number)
                .NotEmpty().WithMessage("Informe o número da solicitação de compra.")
                .MaximumLength(50).WithMessage("O número da solicitação de compra deve ter no máximo 50 caracteres.");

            RuleFor(pr => pr.RequestDate)
                .NotEmpty().WithMessage("Informe a data da solicitação de compra.");

            RuleFor(pr => pr.ExpectedDeliveryDate)
                .GreaterThanOrEqualTo(pr => pr.RequestDate).When(pr => pr.ExpectedDeliveryDate.HasValue)
                .WithMessage("A previsão de entrega não pode ser antes da data da SC.");

            RuleFor(pr => pr.PaymentTerms)
                .MaximumLength(200).WithMessage("A condição de pagamento deve ter no máximo 200 caracteres.");

            RuleFor(pr => pr.RequesterName)
                .MaximumLength(150).WithMessage("O nome do solicitante deve ter no máximo 150 caracteres.");

            RuleFor(pr => pr.Notes)
                .MaximumLength(500).WithMessage("A observação deve ter no máximo 500 caracteres.");

            RuleFor(pr => pr.Items)
                .NotEmpty().WithMessage("Adicione ao menos um produto à SC.")
                .Must(items => items.Select(i => i.ProductId).Distinct().Count() == items.Count)
                .WithMessage("O mesmo produto aparece mais de uma vez na SC. Junte as quantidades em um único item.");

            RuleForEach(pr => pr.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.ProductId)
                    .GreaterThan(0).WithMessage("Selecione o produto de todos os itens.");

                item.RuleFor(i => i.Quantity)
                    .GreaterThan(0).WithMessage("A quantidade de cada item deve ser maior que zero.");

                item.RuleFor(i => i.UnitPrice)
                    .GreaterThanOrEqualTo(0).When(i => i.UnitPrice.HasValue).WithMessage("O preço unitário não pode ser negativo.");
            });
        }
    }
}
