using DeveloperStore.Sales.Domain.Dtos.User;
using FluentValidation;
using System.Diagnostics.CodeAnalysis;

namespace DeveloperStore.Sales.Service.Validations;

[ExcludeFromCodeCoverage]
public class RequestUserValidator : AbstractValidator<RequestUserDto>
{
    public RequestUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .EmailAddress().WithMessage("O e-mail fornecido não é válido.");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("O nome de usuário é obrigatório.")
            .MinimumLength(3).WithMessage("O nome de usuário deve ter pelo menos 3 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("A senha é obrigatória.")
            .MinimumLength(6).WithMessage("A senha deve ter pelo menos 6 caracteres.");

        RuleFor(x => x.Name.Firstname)
            .NotEmpty().WithMessage("O primeiro nome é obrigatório.")
            .MinimumLength(2).WithMessage("O primeiro nome deve ter pelo menos 2 caracteres.");

        RuleFor(x => x.Name.Lastname)
            .NotEmpty().WithMessage("O sobrenome é obrigatório.")
            .MinimumLength(2).WithMessage("O sobrenome deve ter pelo menos 2 caracteres.");

        RuleFor(x => x.Address.City)
            .NotEmpty().WithMessage("A cidade é obrigatória.");

        RuleFor(x => x.Address.Street)
            .NotEmpty().WithMessage("A rua é obrigatória.");

        RuleFor(x => x.Address.Number)
            .GreaterThan(0).WithMessage("O número do endereço deve ser maior que 0.");

        RuleFor(x => x.Address.Zipcode)
            .NotEmpty().WithMessage("O CEP é obrigatório.")
            .Matches(@"^\d{5}-\d{3}$").WithMessage("O CEP deve estar no formato 00000-000.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("O status do usuário é obrigatório.")
            .Must(status => new[] { "Active", "Inactive", "Suspended" }.Contains(status))
            .WithMessage("O status deve ser Active, Inactive ou Suspended.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("O papel do usuário é obrigatório.")
            .Must(role => new[] { "Customer", "Manager", "Admin" }.Contains(role))
            .WithMessage("O papel deve ser Customer, Manager ou Admin.");
    }
}
