using System.Text;
using CinemaTicketsBack.Infrastructure.Database;
using CinemaTicketsBack.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CinemaTicketsBack.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddSingleton<IDatabaseConnectionFactory, SqlServerConnectionFactory>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.Configure<ReservationSettings>(
            configuration.GetSection(ReservationSettings.SectionName));

        var secret = configuration["JwtSettings:Secret"] ??
        throw new InvalidOperationException("JwtSettings:Secret configuration is missing.");
        var issuer = configuration["JwtSettings:Issuer"] ?? "CinemaTicketsAPI";
        var audience = configuration["JwtSettings:Audience"] ?? "CinemaTicketsClient";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
        });

        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>("database_health_check");

        return services;
    }
}
