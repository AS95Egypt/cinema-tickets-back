using CinemaTicketsBack.Infrastructure.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CinemaTicketsBack.Tests.Infrastructure;

public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqlServerContainerFixture _container;
    private readonly string _databaseConnectionString;

    public TestWebApplicationFactory(SqlServerContainerFixture container)
    {
        _container = container;
        _databaseConnectionString = CreateDatabaseConnectionString();

        using var context = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(_databaseConnectionString)
                .Options);
        context.Database.EnsureCreated();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _databaseConnectionString,
                ["JwtSettings:Secret"] = "test-secret-that-is-long-enough-for-jwt-signing-123456789",
                ["JwtSettings:Issuer"] = "CinemaTicketsTests",
                ["JwtSettings:Audience"] = "CinemaTicketsTestClient",
                ["ReservationSettings:HoldDurationMinutes"] = "10",
                ["ReservationSettings:Currency"] = "EGP"
            });
        });

        builder.ConfigureServices(services =>
        {
            var registrations = services
                .Where(descriptor => descriptor.ServiceType == typeof(AppDbContext) ||
                                     descriptor.ServiceType == typeof(DbContextOptions<AppDbContext>))
                .ToList();

            foreach (var registration in registrations)
            {
                services.Remove(registration);
            }

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(_databaseConnectionString));
        });
    }

    private string CreateDatabaseConnectionString()
    {
        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(_container.ConnectionString)
        {
            InitialCatalog = $"CinemaTicketsTests_{Guid.NewGuid():N}"
        };

        return builder.ConnectionString;
    }
}