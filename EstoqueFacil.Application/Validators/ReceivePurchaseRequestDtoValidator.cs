using EstoqueFacil.Application.Dtos;
using FluentValidation;

namespace EstoqueFacil.Application.Validators
{
    public class ReceivePurchaseRequestDtoValidator : AbstractValidator<ReceivePurchaseRequestDto>
    {
        public ReceivePurchaseRequestDtoValidator()
        {
            RuleFor(r => r.Quantity)
                .GreaterThan(0).WithMessage("A quantidade recebida deve ser maior que zero.");

            RuleFor(r => r.ReceivedDate)
                .NotEmpty().WithMessage("Informe a data de recebimento.");
        }
    }
}
