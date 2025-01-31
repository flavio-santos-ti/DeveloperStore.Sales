using DeveloperStore.Sales.Domain.Events;
using DeveloperStore.Sales.Service.Interfaces;
using DeveloperStore.Sales.Service.Services;
using DeveloperStore.Sales.Service.Validations;
using DeveloperStore.Sales.Storage.NoSQL.MongoDb.Contexts;
using DeveloperStore.Sales.Storage.NoSQL.MongoDb.Interfaces;
using DeveloperStore.Sales.Storage.NoSQL.MongoDb.Repositories;
using DeveloperStore.Sales.Storage.SQL.PostgreSQL.Interfaces;
using DeveloperStore.Sales.Storage.SQL.PostgreSQL.Repositories;
using DeveloperStore.Sales.Storage.UnitOfWork;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace DeveloperStore.Sales.Api.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
    {
        services.AddScoped<IMongoDbContext, MongoDbContext>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IEventLogMongoDbRepository, EventLogMongoDbRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<ICartProductRepository, CartProductRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<ISaleItemRepository, SaleItemRepository>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ISaleService, SaleService>();

        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));

        services.AddValidatorsFromAssemblyContaining<RequestProductValidator>();
        services.AddValidatorsFromAssemblyContaining<RequestUserValidator>();
        services.AddValidatorsFromAssemblyContaining<RequestCartValidator>();
        services.AddValidatorsFromAssemblyContaining<RequestCartProductValidator>();
        
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ItemCancelledEvent).Assembly));
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SaleCancelledEvent).Assembly));
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SaleCreatedEvent).Assembly));
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SaleModifiedEvent).Assembly));

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var key = Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero // Para evitar atrasos na expiração do token
            };
        });

        return services;
    }
}
