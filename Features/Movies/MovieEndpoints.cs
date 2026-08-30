using CinemaTicketsBack.DTOs;
using CinemaTicketsBack.Enums;
using CinemaTicketsBack.Infrastructure.Database;
using CinemaTicketsBack.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketsBack.Features.Movies;

public static class MovieEndpoints
{
    public static void MapMovieEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/movies").WithTags("Movies");

        group.MapGet("/active", async (string? view, AppDbContext db) =>
        {
            var isSummary = string.Equals(view, "summary", StringComparison.OrdinalIgnoreCase);
            var query = db.Movies.Where(m => m.IsActive);

            if (isSummary)
            {
                var summaries = await query
                    .Select(m => new MovieSummaryDto(
                        m.Id, m.Title, m.Genre, m.Duration, m.ReleaseDate, m.Language))
                    .ToListAsync();
                return Results.Ok(summaries);
            }

            var detailed = await query
                .Select(m => new MovieDto(
                    m.Id, m.Title, m.Genre, m.Duration, m.ReleaseDate,
                    m.Language, m.Description, m.Actors, m.TrailerUrl,
                    m.IsActive, m.CreatedAt, m.UpdatedAt))
                .ToListAsync();
            return Results.Ok(detailed);
        });

        group.MapGet("", async (MovieGenre? genre, bool? activeOnly, string? title, AppDbContext db) =>
        {
            var query = db.Movies.AsQueryable();

            if (genre.HasValue)
            {
                query = query.Where(m => m.Genre == genre.Value);
            }
            if (activeOnly.HasValue && activeOnly.Value)
            {
                query = query.Where(m => m.IsActive);
            }
            if (!string.IsNullOrWhiteSpace(title))
            {
                query = query.Where(m => m.Title.Contains(title.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            var movies = await query
                .Select(m => new MovieDto(
                    m.Id, m.Title, m.Genre, m.Duration, m.ReleaseDate,
                    m.Language, m.Description, m.Actors, m.TrailerUrl,
                    m.IsActive, m.CreatedAt, m.UpdatedAt))
                .ToListAsync();

            return Results.Ok(movies);
        });

        group.MapGet("/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var movie = await db.Movies.FindAsync(id);
            if (movie is null)
            {
                return Results.NotFound(new { message = "Movie not found." });
            }

            var dto = new MovieDto(
                movie.Id, movie.Title, movie.Genre, movie.Duration, movie.ReleaseDate,
                movie.Language, movie.Description, movie.Actors, movie.TrailerUrl,
                movie.IsActive, movie.CreatedAt, movie.UpdatedAt);
            return Results.Ok(dto);
        });

        group.MapPost("", async (CreateMovieRequest request, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return Results.BadRequest(new { message = "Movie title is required." });
            }
            if (request.Duration <= 0)
            {
                return Results.BadRequest(new { message = "Duration must be greater than zero." });
            }
            if (request.ReleaseDate == default)
            {
                return Results.BadRequest(new { message = "Valid release date is required." });
            }
            if (string.IsNullOrWhiteSpace(request.Language))
            {
                return Results.BadRequest(new { message = "Language is required." });
            }
            if (!string.IsNullOrWhiteSpace(request.TrailerUrl) &&
                !Uri.TryCreate(request.TrailerUrl, UriKind.Absolute, out _))
            {
                return Results.BadRequest(new { message = "Trailer URL must be a valid absolute URL when provided." });
            }

            var movie = new Movie
            {
                Id = Guid.NewGuid(),
                Title = request.Title.Trim(),
                Genre = request.Genre,
                Duration = request.Duration,
                ReleaseDate = request.ReleaseDate,
                Language = request.Language.Trim(),
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                Actors = string.IsNullOrWhiteSpace(request.Actors) ? null : request.Actors.Trim(),
                TrailerUrl = string.IsNullOrWhiteSpace(request.TrailerUrl) ? null : request.TrailerUrl.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            db.Movies.Add(movie);
            await db.SaveChangesAsync();

            var dto = new MovieDto(
                movie.Id, movie.Title, movie.Genre, movie.Duration, movie.ReleaseDate,
                movie.Language, movie.Description, movie.Actors, movie.TrailerUrl,
                movie.IsActive, movie.CreatedAt, movie.UpdatedAt);
            return Results.Created($"/api/v1/movies/{movie.Id}", dto);
        }).RequireAuthorization("AdminOnly");

        group.MapPut("/{id:guid}", async (Guid id, UpdateMovieRequest request, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return Results.BadRequest(new { message = "Movie title is required." });
            }
            if (request.Duration <= 0)
            {
                return Results.BadRequest(new { message = "Duration must be greater than zero." });
            }
            if (request.ReleaseDate == default)
            {
                return Results.BadRequest(new { message = "Valid release date is required." });
            }
            if (string.IsNullOrWhiteSpace(request.Language))
            {
                return Results.BadRequest(new { message = "Language is required." });
            }
            if (!string.IsNullOrWhiteSpace(request.TrailerUrl) &&
                !Uri.TryCreate(request.TrailerUrl, UriKind.Absolute, out _))
            {
                return Results.BadRequest(new { message = "Trailer URL must be a valid absolute URL when provided." });
            }

            var movie = await db.Movies.FindAsync(id);
            if (movie is null)
            {
                return Results.NotFound(new { message = "Movie not found." });
            }

            movie.Title = request.Title.Trim();
            movie.Genre = request.Genre;
            movie.Duration = request.Duration;
            movie.ReleaseDate = request.ReleaseDate;
            movie.Language = request.Language.Trim();
            movie.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            movie.Actors = string.IsNullOrWhiteSpace(request.Actors) ? null : request.Actors.Trim();
            movie.TrailerUrl = string.IsNullOrWhiteSpace(request.TrailerUrl) ? null : request.TrailerUrl.Trim();
            movie.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            var dto = new MovieDto(
                movie.Id, movie.Title, movie.Genre, movie.Duration, movie.ReleaseDate,
                movie.Language, movie.Description, movie.Actors, movie.TrailerUrl,
                movie.IsActive, movie.CreatedAt, movie.UpdatedAt);
            return Results.Ok(dto);
        }).RequireAuthorization("AdminOnly");

        group.MapPatch("/{id:guid}/deactivate", async (Guid id, AppDbContext db) =>
        {
            var movie = await db.Movies.FindAsync(id);
            if (movie is null)
            {
                return Results.NotFound(new { message = "Movie not found." });
            }

            movie.IsActive = false;
            movie.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            var dto = new MovieDto(
                movie.Id, movie.Title, movie.Genre, movie.Duration, movie.ReleaseDate,
                movie.Language, movie.Description, movie.Actors, movie.TrailerUrl,
                movie.IsActive, movie.CreatedAt, movie.UpdatedAt);
            return Results.Ok(dto);
        }).RequireAuthorization("AdminOnly");

        group.MapPatch("/{id:guid}/activate", async (Guid id, AppDbContext db) =>
        {
            var movie = await db.Movies.FindAsync(id);
            if (movie is null)
            {
                return Results.NotFound(new { message = "Movie not found." });
            }

            movie.IsActive = true;
            movie.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            var dto = new MovieDto(
                movie.Id, movie.Title, movie.Genre, movie.Duration, movie.ReleaseDate,
                movie.Language, movie.Description, movie.Actors, movie.TrailerUrl,
                movie.IsActive, movie.CreatedAt, movie.UpdatedAt);
            return Results.Ok(dto);
        }).RequireAuthorization("AdminOnly");
    }
}
