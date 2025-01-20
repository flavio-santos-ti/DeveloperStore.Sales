using AutoMapper;
using DeveloperStore.Sales.Domain.Dtos.Cart;
using DeveloperStore.Sales.Domain.Models;

namespace DeveloperStore.Sales.Service.Mappings;

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
