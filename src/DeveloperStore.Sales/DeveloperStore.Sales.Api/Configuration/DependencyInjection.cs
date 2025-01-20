using DeveloperStore.Sales.Service.Interfaces;
using DeveloperStore.Sales.Service.Services;
using DeveloperStore.Sales.Service.Validations;
using DeveloperStore.Sales.Storage.Interfaces;
using DeveloperStore.Sales.Storage.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;

namespace DeveloperStore.Sales.Api.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductService, ProductService>();
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        //services.AddFluentValidationAutoValidation();

        services.AddValidatorsFromAssemblyContaining<RequestProductValidator>();


        return services;
    }

}
