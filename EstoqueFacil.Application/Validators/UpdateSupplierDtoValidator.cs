using EstoqueFacil.Application.Dtos;
using FluentValidation;

namespace EstoqueFacil.Application.Validators
{
    public class UpdateSupplierDtoValidator : AbstractValidator<UpdateSupplierDto>
    {
        public UpdateSupplierDtoValidator()
        {
            RuleFor(s => s.Name)
                .NotEmpty().WithMessage("O nome do fornecedor é obrigatório.")
                .MaximumLength(150).WithMessage("O nome do fornecedor deve ter no máximo 150 caracteres.");

            RuleFor(s => s.Document)
                .MaximumLength(20).WithMessage("O documento deve ter no máximo 20 caracteres.");

            RuleFor(s => s.Email)
                .EmailAddress().When(s => !string.IsNullOrWhiteSpace(s.Email))
                .WithMessage("Informe um e-mail válido.")
                .MaximumLength(150).WithMessage("O e-mail deve ter no máximo 150 caracteres.");

            RuleFor(s => s.Phone)
                .MaximumLength(20).WithMessage("O telefone deve ter no máximo 20 caracteres.");

            RuleFor(s => s.Address)
                .MaximumLength(300).WithMessage("O endereço deve ter no máximo 300 caracteres.");
        }
    }
}
