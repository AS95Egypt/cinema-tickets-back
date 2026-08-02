using CinemaTicketsBack.Infrastructure.Database;

namespace CinemaTicketsBack.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDatabaseConnectionFactory, SqlServerConnectionFactory>();
        return services;
    }
}
