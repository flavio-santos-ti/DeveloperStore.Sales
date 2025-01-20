using DeveloperStore.Sales.Domain.Dtos.Cart;
using FluentValidation;

namespace DeveloperStore.Sales.Service.Validations;

public class RequestCartProductValidator : AbstractValidator<RequestCartProductDto>
{
    public RequestCartProductValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0).WithMessage("O ProductId deve ser maior que 0.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("A quantidade deve ser maior que 0.");
    }
}
