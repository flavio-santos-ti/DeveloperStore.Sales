using DeveloperStore.Sales.Storage.SQL.PostgreSQL.Contexts;
using DeveloperStore.Sales.Storage.SQL.PostgreSQL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeveloperStore.Sales.Api.Configuration;

public static class DatabaseConfiguration
{
    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        services.AddDbContext<PostgreSqlDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IPostgreSqlDbContext, PostgreSqlDbContext>();

        return services;
    }
}
