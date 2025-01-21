using AutoMapper;
using DeveloperStore.Sales.Domain.Dtos.Product;
using DeveloperStore.Sales.Domain.Models;
using System.Diagnostics.CodeAnalysis;

namespace DeveloperStore.Sales.Service.Mappings;

[ExcludeFromCodeCoverage]

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<RequestProductDto, Product>();

        CreateMap<Product, ProductDto>();
    }
}
