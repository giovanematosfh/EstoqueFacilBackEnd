using EstoqueFacil.Application.Dtos;
using FluentValidation;

namespace EstoqueFacil.Application.Validators
{
    public class SaveSectorDtoValidator : AbstractValidator<SaveSectorDto>
    {
        public SaveSectorDtoValidator()
        {
            RuleFor(s => s.Name)
                .NotEmpty().WithMessage("O nome do setor é obrigatório.")
                .MaximumLength(100).WithMessage("O nome do setor deve ter no máximo 100 caracteres.");

            RuleFor(s => s.ResponsibleName)
                .MaximumLength(150).WithMessage("O nome do responsável deve ter no máximo 150 caracteres.");
        }
    }
}
