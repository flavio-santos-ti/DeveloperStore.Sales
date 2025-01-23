using AutoMapper;
using DeveloperStore.Sales.Domain.Dtos.Sale;
using DeveloperStore.Sales.Domain.Models;

namespace DeveloperStore.Sales.Service.Mappings;

public class SaleMappingProfile : Profile
{
    public SaleMappingProfile()
    {
        // Configuração de mapeamento entre Sale e SaleDto
        CreateMap<Sale, SaleDto>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items)); // Mapeia a lista de itens

        // Configuração de mapeamento entre SaleItem e SaleItemDto
        CreateMap<SaleItem, SaleItemDto>();
    }
}
