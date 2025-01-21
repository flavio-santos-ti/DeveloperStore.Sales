using DeveloperStore.Sales.Domain.Dtos.Product;
using FluentValidation;
using System.Diagnostics.CodeAnalysis;

namespace DeveloperStore.Sales.Service.Validations;

[ExcludeFromCodeCoverage]
public class RequestProductValidator : AbstractValidator<RequestProductDto>
{
    public RequestProductValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("O título é obrigatório.")
            .MaximumLength(100).WithMessage("O título não pode exceder 100 caracteres.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("O preço deve ser maior que zero.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("A descrição é obrigatória.")
            .MaximumLength(500).WithMessage("A descrição não pode exceder 500 caracteres.");

        RuleFor(x => x.Image)
            .NotEmpty().WithMessage("A URL da imagem é obrigatória.")
            .Must(image => Uri.TryCreate(image, UriKind.Absolute, out _))
            .WithMessage("A URL da imagem deve ser válida.");
    }
}
