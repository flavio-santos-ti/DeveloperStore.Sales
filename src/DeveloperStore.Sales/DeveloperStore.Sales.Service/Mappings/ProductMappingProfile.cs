using AutoMapper;
using DeveloperStore.Sales.Domain.Dtos.Product;
using DeveloperStore.Sales.Domain.Models;

namespace DeveloperStore.Sales.Service.Mappings;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<RequestProductDto, Product>();

        CreateMap<Product, ProductDto>();
    }
}
