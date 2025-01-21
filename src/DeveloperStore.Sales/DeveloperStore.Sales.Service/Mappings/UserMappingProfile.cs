using AutoMapper;
using DeveloperStore.Sales.Domain.Dtos.User;
using DeveloperStore.Sales.Domain.Models;
using System.Diagnostics.CodeAnalysis;

namespace DeveloperStore.Sales.Service.Mappings;

[ExcludeFromCodeCoverage]
public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<RequestUserDto, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.Firstname, opt => opt.MapFrom(src => src.Name.Firstname))
            .ForMember(dest => dest.Lastname, opt => opt.MapFrom(src => src.Name.Lastname))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.Address.City))
            .ForMember(dest => dest.Street, opt => opt.MapFrom(src => src.Address.Street))
            .ForMember(dest => dest.AddressNumber, opt => opt.MapFrom(src => src.Address.Number))
            .ForMember(dest => dest.Zipcode, opt => opt.MapFrom(src => src.Address.Zipcode))
            .ForMember(dest => dest.GeolocationLat, opt => opt.MapFrom(src => src.Address.Geolocation.Lat))
            .ForMember(dest => dest.GeolocationLong, opt => opt.MapFrom(src => src.Address.Geolocation.Long));

        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => new NameDto { Firstname = src.Firstname, Lastname = src.Lastname }))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => new AddressDto
            {
                City = src.City,
                Street = src.Street,
                Number = src.AddressNumber,
                Zipcode = src.Zipcode,
                Geolocation = new GeolocationDto { Lat = src.GeolocationLat, Long = src.GeolocationLong }
            }));
    }
}
