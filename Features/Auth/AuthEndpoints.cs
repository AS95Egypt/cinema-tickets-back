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
        var group = app.MapGroup("/auth").WithTags("Auth");

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

        // TODO add logout endpoint
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
