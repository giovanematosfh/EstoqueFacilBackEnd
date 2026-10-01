using EstoqueFacil.Application.Dtos;
using FluentValidation;

namespace EstoqueFacil.Application.Validators
{
    public class UpdatePurchaseRequestDtoValidator : AbstractValidator<UpdatePurchaseRequestDto>
    {
        public UpdatePurchaseRequestDtoValidator()
        {
            RuleFor(pr => pr.RequestNumber)
                .NotEmpty().WithMessage("O número da solicitação de compra é obrigatório.")
                .MaximumLength(50).WithMessage("O número da solicitação de compra deve ter no máximo 50 caracteres.");

            RuleFor(pr => pr.RequestDate)
                .NotEqual(default(DateTime)).WithMessage("Informe a data da solicitação.");
        }
    }
}
