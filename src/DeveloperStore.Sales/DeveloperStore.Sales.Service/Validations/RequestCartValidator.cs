using DeveloperStore.Sales.Domain.Dtos.Cart;
using FluentValidation;

namespace DeveloperStore.Sales.Service.Validations;

public class RequestCartValidator : AbstractValidator<RequestCartDto>
{
    public RequestCartValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("O UserId deve ser maior que 0.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("A data do carrinho é obrigatória.");

        RuleForEach(x => x.Products).SetValidator(new RequestCartProductValidator());
    }
}
