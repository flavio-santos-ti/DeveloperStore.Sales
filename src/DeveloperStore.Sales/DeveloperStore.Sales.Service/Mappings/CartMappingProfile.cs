using AutoMapper;
using DeveloperStore.Sales.Domain.Dtos.Cart;
using DeveloperStore.Sales.Domain.Models;
using System.Diagnostics.CodeAnalysis;

namespace DeveloperStore.Sales.Service.Mappings;

[ExcludeFromCodeCoverage]
public class CartMappingProfile : Profile
{
    public CartMappingProfile()
    {
        CreateMap<RequestCartDto, Cart>();
        CreateMap<RequestCartProductDto, CartProduct>();

        CreateMap<Cart, CartDto>();
        CreateMap<CartProduct, CartProductDto>();
    }
}
