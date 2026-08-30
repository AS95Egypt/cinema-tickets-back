# Story 02 — User Story 2 — User Registration and Authentication (Story: 2)

## Prerequisites
- [Story 01 completed](file:///d:/AZM/Full%20stack%20program/CinemaTicketsBack/.squad/plans/project-init-infra/01-story-initialize-cinema-reservation-api-1.md): ASP.NET Core 8.0 API project initialized with EF Core `AppDbContext` and SQL Server infrastructure.

## Story Goal
Implement secure user registration (`POST /api/auth/register`) and authentication (`POST /api/auth/login`) with BCrypt password hashing, JWT token generation, role-based authorization claims, JWT Bearer authentication middleware, EF Core `User` entity & database migration, and unit/integration test coverage.

## Context — Read These Files First
1. `cinema-tickets-back.csproj` — lines 1–27. Targets `net8.0`. Needs NuGet package references for `Microsoft.AspNetCore.Authentication.JwtBearer` (version 8.0.8) and `BCrypt.Net-Next` (version 4.0.3).
2. `Program.cs` — lines 1–30. Main entry point. Needs `app.UseAuthentication()` and `app.UseAuthorization()` middleware invoked before `app.MapApplicationEndpoints()`.
3. `appsettings.json` — lines 1–12. Configuration file. Needs `JwtSettings` section containing `Secret`, `Issuer`, `Audience`, and `ExpiryMinutes`.
4. `Infrastructure/Database/AppDbContext.cs` — lines 1–28. EF Core DbContext. Needs `DbSet<User> Users` property and entity mapping in `OnModelCreating`.
5. `Extensions/ServiceCollectionExtensions.cs` — lines 1–24. Service registrations. Needs JWT Authentication scheme configuration (`AddAuthentication().AddJwtBearer()`), Authorization policies (`AddAuthorization()`), and DI registrations for `IPasswordHasher` and `IJwtTokenGenerator`.
6. `Extensions/EndpointRouteBuilderExtensions.cs` — lines 1–14. Endpoint routing extension. Needs `app.MapAuthEndpoints()`.
7. `../project-init-infra/01-story-initialize-cinema-reservation-api-1.md` — Precedent plan detailing project architecture and testing setup.

## Product rules (from story)
- **Current behaviour**: Application has no authentication system, user entity, or authorization checks.
- **New behaviour**: 
  - `POST /api/auth/register` creates a user with hashed password (never plain-text), `IsAdmin = false` by default, and `IsActive = true` by default. Rejects duplicate emails and invalid inputs.
  - `POST /api/auth/login` validates credentials against stored BCrypt password hash, verifies active status, and returns a signed JWT access token with user details. Rejects inactive accounts and invalid credentials without leaking email existence.
  - Endpoints can enforce JWT authentication (`[Authorize]`) or role-based claims (`Admin`).

## Implementation body

### 1 — Add NuGet Packages
File: `cinema-tickets-back.csproj`
Add package references under `<ItemGroup>`:
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.8" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
```

### 2 — Configure JWT Settings in appsettings.json
File: `appsettings.json`
Add `JwtSettings` configuration block:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=.\\SQLEXPRESS;Persist Security Info=False;User ID=sa;Pooling=False;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=False;Application Name=\"SQL Server Management Studio\";Command Timeout=0"
  },
  "JwtSettings": {
    "Secret": "SuperSecretKeyForCinemaTicketsApiJwtSigningMustBeAtLeast32BytesLong!",
    "Issuer": "CinemaTicketsAPI",
    "Audience": "CinemaTicketsClient",
    "ExpiryMinutes": 60
  }
}
```

### 3 — Create User Entity Model
Create file: `Models/User.cs`
Define the `User` domain model:
```csharp
namespace CinemaTicketsBack.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsAdmin { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

### 4 — Create Auth DTOs
Create file: `DTOs/AuthDtos.cs`
Define request and response DTOs:
```csharp
namespace CinemaTicketsBack.DTOs;

public record RegisterRequest(string Username, string Email, string Password);

public record LoginRequest(string Email, string Password);

public record UserDto(Guid Id, string Username, string Email, bool IsAdmin);

public record AuthResponse(string AccessToken, int ExpiresIn, UserDto User);
```

### 5 — Create Password Hasher Service
Create file: `Services/IPasswordHasher.cs`
```csharp
namespace CinemaTicketsBack.Services;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}
```

Create file: `Services/PasswordHasher.cs`
Implement using `BCrypt.Net.BCrypt`:
```csharp
namespace CinemaTicketsBack.Services;

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
```

### 6 — Create JWT Token Generator Service
Create file: `Services/IJwtTokenGenerator.cs`
```csharp
using CinemaTicketsBack.Models;

namespace CinemaTicketsBack.Services;

public interface IJwtTokenGenerator
{
    (string Token, int ExpiresIn) GenerateToken(User user);
}
```

Create file: `Services/JwtTokenGenerator.cs`
Implement JWT creation:
```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CinemaTicketsBack.Models;
using Microsoft.IdentityModel.Tokens;

namespace CinemaTicketsBack.Services;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, int ExpiresIn) GenerateToken(User user)
    {
        var secret = _configuration["JwtSettings:Secret"]
            ?? throw new InvalidOperationException("JwtSettings:Secret configuration is missing.");
        var issuer = _configuration["JwtSettings:Issuer"] ?? "CinemaTicketsAPI";
        var audience = _configuration["JwtSettings:Audience"] ?? "CinemaTicketsClient";
        var expiryMinutes = int.TryParse(_configuration["JwtSettings:ExpiryMinutes"], out var exp) ? exp : 60;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.Username),
            new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User"),
            new Claim("isAdmin", user.IsAdmin.ToString().ToLower())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(securityToken);

        return (tokenString, expiryMinutes * 60);
    }
}
```

### 7 — Update AppDbContext with Users DbSet
File: `Infrastructure/Database/AppDbContext.cs`
Update `AppDbContext` to map `User` entity:
```csharp
using CinemaTicketsBack.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketsBack.Infrastructure.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Genre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DurationMinutes).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.IsAdmin).HasDefaultValue(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });
    }
}
```

### 8 — Update ServiceCollectionExtensions for Auth & JWT Bearer
File: `Extensions/ServiceCollectionExtensions.cs`
Register `IPasswordHasher`, `IJwtTokenGenerator`, and configure JWT Authentication & Authorization policies:
```csharp
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

        var secret = configuration["JwtSettings:Secret"] ?? "SuperSecretKeyForCinemaTicketsApiJwtSigningMustBeAtLeast32BytesLong!";
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
```

### 9 — Create Auth Endpoints
Create file: `Features/Auth/AuthEndpoints.cs`
Implement registration and login endpoints:
```csharp
using System.Net.Mail;
using CinemaTicketsBack.DTOs;
using CinemaTicketsBack.Infrastructure.Database;
using CinemaTicketsBack.Models;
using CinemaTicketsBack.Services;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketsBack.Features.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", async (RegisterRequest request, AppDbContext db, IPasswordHasher passwordHasher) =>
        {
            if (string.IsNullOrWhiteSpace(request.Username))
            {
                return Results.BadRequest(new { message = "Username is required." });
            }

            if (string.IsNullOrWhiteSpace(request.Email) || !IsValidEmail(request.Email))
            {
                return Results.BadRequest(new { message = "Valid email address is required." });
            }

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            {
                return Results.BadRequest(new { message = "Password must be at least 6 characters long." });
            }

            var normalizedEmail = request.Email.Trim().ToLower();

            var emailExists = await db.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail);
            if (emailExists)
            {
                return Results.BadRequest(new { message = "Email is already registered." });
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username.Trim(),
                Email = normalizedEmail,
                PasswordHash = passwordHasher.HashPassword(request.Password),
                IsAdmin = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            var userDto = new UserDto(user.Id, user.Username, user.Email, user.IsAdmin);
            return Results.Created($"/api/users/{user.Id}", userDto);
        });

        group.MapPost("/login", async (LoginRequest request, AppDbContext db, IPasswordHasher passwordHasher, IJwtTokenGenerator tokenGenerator) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.BadRequest(new { message = "Email and password are required." });
            }

            var normalizedEmail = request.Email.Trim().ToLower();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

            if (user is null || !passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Results.Json(new { message = "Invalid email or password." }, statusCode: 401);
            }

            if (!user.IsActive)
            {
                return Results.Json(new { message = "User account is inactive." }, statusCode: 401);
            }

            var (token, expiresIn) = tokenGenerator.GenerateToken(user);
            var userDto = new UserDto(user.Id, user.Username, user.Email, user.IsAdmin);

            return Results.Ok(new AuthResponse(token, expiresIn, userDto));
        });
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
```

### 10 — Map Auth Endpoints in EndpointRouteBuilderExtensions
File: `Extensions/EndpointRouteBuilderExtensions.cs`
Update `MapApplicationEndpoints`:
```csharp
using CinemaTicketsBack.Features.Auth;
using CinemaTicketsBack.Features.Movies;

namespace CinemaTicketsBack.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapApplicationEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapMovieEndpoints();
        app.MapAuthEndpoints();
        app.MapHealthChecks("/health");
        return app;
    }
}
```

### 11 — Update Program.cs for Authentication and Authorization Middleware
File: `Program.cs`
Add `app.UseAuthentication()` and `app.UseAuthorization()` before endpoint mapping:
```csharp
using CinemaTicketsBack.Extensions;
using CinemaTicketsBack.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new { message = "Cinema Tickets API is running." }));
app.MapApplicationEndpoints();

app.Run();

public partial class Program { }
```

### 12 — Create EF Core Migration for Users Table
Execute `dotnet ef migrations add AddUsersTable` in the root directory to generate the database schema migration for `Users`.

---

## Edge Cases & Failure Modes
- **Duplicate Email Registration**: Triggered when `request.Email` matches an existing record in `Users`. Enforced in `AuthEndpoints.cs` via `db.Users.AnyAsync(...)`. Returns HTTP 400 Bad Request ("Email is already registered.").
- **Invalid Credentials on Login**: Triggered when user is not found or BCrypt verification fails. Enforced in `AuthEndpoints.cs`. Returns HTTP 401 Unauthorized ("Invalid email or password.") without revealing whether the email exists.
- **Inactive User Account Login**: Triggered when `user.IsActive == false`. Enforced in `AuthEndpoints.cs`. Returns HTTP 401 Unauthorized ("User account is inactive.").
- **Malformed Email / Empty Inputs**: Triggered when email format is invalid or required fields are missing. Enforced in `AuthEndpoints.cs` via `IsValidEmail()` check and string validation. Returns HTTP 400 Bad Request.
- **Short or Weak Password**: Triggered when registration password length is less than 6 characters. Enforced in `AuthEndpoints.cs`. Returns HTTP 400 Bad Request ("Password must be at least 6 characters long.").
- **Expired or Malformed JWT Token**: Triggered on protected endpoints when token signature, issuer, or expiration check fails. Enforced by ASP.NET Core `JwtBearer` middleware returning HTTP 401 Unauthorized.
- **Non-Admin Accessing Admin Policy Endpoint**: Triggered when normal user token is passed to endpoint requiring `AdminOnly` policy. Enforced by ASP.NET Core Authorization middleware returning HTTP 403 Forbidden.

---

## Test Plan
1. **Unit Test - PasswordHasherTests**:
   File: `cinema-tickets-back.Tests/Services/PasswordHasherTests.cs`
   - `HashPassword_ProducesValidBCryptHashAndVerifies`: Verifies hashing and verification.
2. **Unit Test - JwtTokenGeneratorTests**:
   File: `cinema-tickets-back.Tests/Services/JwtTokenGeneratorTests.cs`
   - `GenerateToken_ReturnsValidSignedTokenWithUserClaims`: Verifies JWT payload claims (`sub`, `email`, `role`, `isAdmin`).
3. **Integration Test - AuthEndpointsTests**:
   File: `cinema-tickets-back.Tests/Endpoints/AuthEndpointsTests.cs`
   - `Register_ValidUser_Returns201CreatedAndUserDto`: Registers user without plain-text password.
   - `Register_DuplicateEmail_Returns400BadRequest`: Rejects duplicate email.
   - `Login_ValidCredentials_Returns200WithAccessToken`: Validates successful login and JWT payload.
   - `Login_InvalidPassword_Returns401Unauthorized`: Rejects invalid credentials.
   - `Login_InactiveUser_Returns401Unauthorized`: Rejects inactive user login.

---

## Migration / Rollback
- **Migration**: Run `dotnet ef migrations add AddUsersTable` to generate `Migrations/<timestamp>_AddUsersTable.cs`. Run `dotnet ef database update` to apply migration to SQL Server.
- **Rollback**: Run `dotnet ef database update InitialCreate` to revert `Users` table migration, then remove the `AddUsersTable` migration files.

---

## Verification Steps
1. **Backend builds:** Run `dotnet build` in root directory `.`.
2. **EF Core migration creation:** Run `dotnet ef migrations add AddUsersTable` in root directory `.`.
3. **Test suite execution:** Run `dotnet test cinema-tickets-back.Tests/cinema-tickets-back.Tests.csproj` in root directory `.`.
4. **Smoke test registration:** `POST /api/auth/register` with valid JSON payload.
5. **Smoke test login:** `POST /api/auth/login` with registered credentials, verify `accessToken` returned.

---

## Done Criteria
- [ ] `POST /api/auth/register` registers a new user with BCrypt hashed password, `IsAdmin = false`, and `IsActive = true`.
- [ ] Duplicate email registration is rejected with HTTP 400 Bad Request.
- [ ] Passwords are never stored in plain text in the database.
- [ ] `POST /api/auth/login` verifies credentials and active status, returning HTTP 401 Unauthorized for invalid credentials or inactive users.
- [ ] Successful login returns JWT access token (`accessToken`), `expiresIn`, and user basic info (`UserDto`).
- [ ] JWT authentication and authorization middleware configured in ASP.NET Core pipeline.
- [ ] EF Core `AddUsersTable` migration created and builds cleanly.
- [ ] All unit and integration tests in test suite pass cleanly.
