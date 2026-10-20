using EstoqueFacil.Application.Dtos;
using FluentValidation;

namespace EstoqueFacil.Application.Validators
{
    public class ReceivePurchaseRequestDtoValidator : AbstractValidator<ReceivePurchaseRequestDto>
    {
        public ReceivePurchaseRequestDtoValidator()
        {
            RuleFor(r => r.ReceivedDate)
                .NotEmpty().WithMessage("Informe a data de recebimento.");

            RuleFor(r => r.Items)
                .Must(items => items.Any(i => i.Quantity > 0))
                .WithMessage("Informe a quantidade recebida de ao menos um item.");

            RuleForEach(r => r.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.Quantity)
                    .GreaterThanOrEqualTo(0).WithMessage("A quantidade recebida não pode ser negativa.");
            });
        }
    }
}
