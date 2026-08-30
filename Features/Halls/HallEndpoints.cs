using CinemaTicketsBack.DTOs;
using CinemaTicketsBack.Infrastructure.Database;
using CinemaTicketsBack.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketsBack.Features.Halls;

public static class HallEndpoints
{
    public static void MapHallEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/halls").WithTags("Halls");

        // GET /api/v1/halls
        group.MapGet("", async (bool? activeOnly, AppDbContext db) =>
        {
            var query = db.Halls.AsQueryable();
            if (activeOnly.HasValue && activeOnly.Value)
            {
                query = query.Where(h => h.IsActive);
            }

            var halls = await query
                .Select(h => new HallDto(h.Id, h.Title, h.NumberOfSeats, h.Type, h.IsActive, h.CreatedAt, h.UpdatedAt))
                .ToListAsync();

            return Results.Ok(halls);
        });

        // GET /api/v1/halls/{id}
        group.MapGet("/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var hall = await db.Halls.FindAsync(id);
            if (hall is null)
            {
                return Results.NotFound(new { message = "Cinema hall not found." });
            }

            var hallDto = new HallDto(hall.Id, hall.Title, hall.NumberOfSeats, hall.Type, hall.IsActive, hall.CreatedAt, hall.UpdatedAt);
            return Results.Ok(hallDto);
        });

        // POST /api/v1/halls (Admin Only)
        group.MapPost("", async (CreateHallRequest request, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return Results.BadRequest(new { message = "Hall title is required." });
            }

            if (request.NumberOfSeats <= 0)
            {
                return Results.BadRequest(new { message = "Number of seats must be greater than zero." });
            }

            var hall = new Hall
            {
                Id = Guid.NewGuid(),
                Title = request.Title.Trim(),
                NumberOfSeats = request.NumberOfSeats,
                Type = request.Type,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            db.Halls.Add(hall);
            await db.SaveChangesAsync();

            var hallDto = new HallDto(hall.Id, hall.Title, hall.NumberOfSeats, hall.Type, hall.IsActive, hall.CreatedAt, hall.UpdatedAt);
            return Results.Created($"/api/v1/halls/{hall.Id}", hallDto);
        }).RequireAuthorization("AdminOnly");

        // PUT /api/v1/halls/{id} (Admin Only)
        group.MapPut("/{id:guid}", async (Guid id, UpdateHallRequest request, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return Results.BadRequest(new { message = "Hall title is required." });
            }

            if (request.NumberOfSeats <= 0)
            {
                return Results.BadRequest(new { message = "Number of seats must be greater than zero." });
            }

            var hall = await db.Halls.FindAsync(id);
            if (hall is null)
            {
                return Results.NotFound(new { message = "Cinema hall not found." });
            }

            hall.Title = request.Title.Trim();
            hall.NumberOfSeats = request.NumberOfSeats;
            hall.Type = request.Type;
            hall.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            var hallDto = new HallDto(hall.Id, hall.Title, hall.NumberOfSeats, hall.Type, hall.IsActive, hall.CreatedAt, hall.UpdatedAt);
            return Results.Ok(hallDto);
        }).RequireAuthorization("AdminOnly");

        // PATCH /api/v1/halls/{id}/deactivate (Admin Only)
        group.MapPatch("/{id:guid}/deactivate", async (Guid id, AppDbContext db) =>
        {
            var hall = await db.Halls.FindAsync(id);
            if (hall is null)
            {
                return Results.NotFound(new { message = "Cinema hall not found." });
            }

            hall.IsActive = false;
            hall.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            var hallDto = new HallDto(hall.Id, hall.Title, hall.NumberOfSeats, hall.Type, hall.IsActive, hall.CreatedAt, hall.UpdatedAt);
            return Results.Ok(hallDto);
        }).RequireAuthorization("AdminOnly");

        // PATCH /api/v1/halls/{id}/activate (Admin Only)
        group.MapPatch("/{id:guid}/activate", async (Guid id, AppDbContext db) =>
        {
            var hall = await db.Halls.FindAsync(id);
            if (hall is null)
            {
                return Results.NotFound(new { message = "Cinema hall not found." });
            }

            hall.IsActive = true;
            hall.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            var hallDto = new HallDto(hall.Id, hall.Title, hall.NumberOfSeats, hall.Type, hall.IsActive, hall.CreatedAt, hall.UpdatedAt);
            return Results.Ok(hallDto);
        }).RequireAuthorization("AdminOnly");
    }
}
