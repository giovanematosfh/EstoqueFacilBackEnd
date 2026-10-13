using EstoqueFacil.Application.Dtos;
using FluentValidation;

namespace EstoqueFacil.Application.Validators
{
    public class UpdateCompanyDtoValidator : AbstractValidator<UpdateCompanyDto>
    {
        public UpdateCompanyDtoValidator()
        {
            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("O nome da empresa é obrigatório.")
                .MaximumLength(150).WithMessage("O nome da empresa deve ter no máximo 150 caracteres.");

            RuleFor(c => c.Document).MaximumLength(20).WithMessage("O documento deve ter no máximo 20 caracteres.");
            RuleFor(c => c.Email).EmailAddress().When(c => !string.IsNullOrWhiteSpace(c.Email)).WithMessage("Informe um e-mail válido para a empresa.");
            RuleFor(c => c.Phone).MaximumLength(20).WithMessage("O telefone deve ter no máximo 20 caracteres.");
            RuleFor(c => c.PlanId).GreaterThan(0).WithMessage("Selecione um plano.");
        }
    }
}
