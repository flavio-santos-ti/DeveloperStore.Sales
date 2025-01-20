using DeveloperStore.Sales.Service.Interfaces;
using DeveloperStore.Sales.Service.Services;
using DeveloperStore.Sales.Service.Validations;
using DeveloperStore.Sales.Storage.Interfaces;
using DeveloperStore.Sales.Storage.Repositories;
using FluentValidation;

namespace DeveloperStore.Sales.Api.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IUserService, UserService>();
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        services.AddValidatorsFromAssemblyContaining<RequestProductValidator>();
        services.AddValidatorsFromAssemblyContaining<RequestUserValidator>();

        return services;
    }

}
