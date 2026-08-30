using CinemaTicketsBack.DTOs;
using CinemaTicketsBack.Enums;
using CinemaTicketsBack.Infrastructure.Database;
using CinemaTicketsBack.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketsBack.Features.Screenings;

public static class ScreeningEndpoints
{
    public static void MapScreeningEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/movies/{movieId:guid}/screenings")
                        .WithTags("Screenings");
        var seatsGroup = app.MapGroup("/screenings")
                            .WithTags("Screenings");

        group.MapGet("", async (Guid movieId, bool? includePast, AppDbContext db) =>
        {
            var movie = await db.Movies.FindAsync(movieId);
            if (movie is null)
            {
                return Results.NotFound(new { message = "Movie not found." });
            }

            var query = db.Screenings.Where(s => s.MovieId == movieId);

            if (!includePast.HasValue || !includePast.Value)
            {
                query = query.Where(s => s.StartDateTime > DateTime.UtcNow);
            }

            var screenings = await query
                .Include(s => s.Hall)
                .OrderBy(s => s.StartDateTime)
                .Select(s => new ScreeningDto(
                    s.Id,
                    s.StartDateTime,
                    s.Price,
                    new ScreeningHallInfoDto(s.Hall.Id, s.Hall.Title, s.Hall.Type)))
                .ToListAsync();

            var wrapper = new MovieWithScreeningsDto(movie.Id, movie.Title, screenings);
            return Results.Ok(wrapper);
        });

        group.MapPost("", async (Guid movieId, CreateScreeningRequest request, AppDbContext db) =>
        {
            // print request to console for debugging
            Console.WriteLine($"Request: {request}");
            var movie = await db.Movies.FindAsync(movieId);
            if (movie is null)
            {
                return Results.NotFound(new { message = "Movie not found." });
            }
            if (!movie.IsActive)
            {
                return Results.BadRequest(new { message = "Cannot create a screening for an inactive movie." });
            }

            var hall = await db.Halls.FindAsync(request.HallId);
            if (hall is null)
            {
                return Results.NotFound(new { message = "Hall not found." });
            }
            if (!hall.IsActive)
            {
                return Results.BadRequest(new { message = "Cannot create a screening in an inactive hall." });
            }

            if (request.StartDateTime <= DateTime.UtcNow.AddMinutes(-1))
            {
                return Results.BadRequest(new { message = "StartDateTime must be in the future." });
            }

            if (request.Price <= 0)
            {
                return Results.BadRequest(new { message = "Price must be greater than zero." });
            }

            var newEnd = request.StartDateTime.AddMinutes(movie.Duration);

            var hasConflict = await db.Screenings
                .Where(s => s.HallId == request.HallId)
                .Join(db.Movies,
                      s => s.MovieId,
                      m => m.Id,
                      (s, m) => new { s.StartDateTime, Duration = m.Duration })
                .AnyAsync(candidate =>
                    candidate.StartDateTime < newEnd &&
                    request.StartDateTime < candidate.StartDateTime.AddMinutes(candidate.Duration));

            if (hasConflict)
            {
                return Results.BadRequest(new { message = "Screening conflicts with an existing screening in the same hall." });
            }

            var screening = new Screening
            {
                Id = Guid.NewGuid(),
                MovieId = movieId,
                HallId = request.HallId,
                StartDateTime = request.StartDateTime,
                Price = request.Price,
                CreatedAt = DateTime.UtcNow
            };

            db.Screenings.Add(screening);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Results.BadRequest(new { message = "Screening conflicts with an existing screening in the same hall." });
            }

            var created = await db.Screenings
                .Include(s => s.Hall)
                .Where(s => s.Id == screening.Id)
                .Select(s => new ScreeningDto(
                    s.Id, s.StartDateTime, s.Price,
                    new ScreeningHallInfoDto(s.Hall.Id, s.Hall.Title, s.Hall.Type)))
                .FirstAsync();

            return Results.Created($"/api/v1/movies/{movieId}/screenings#{created.Id}", created);
        }).RequireAuthorization("AdminOnly");

        seatsGroup.MapGet("/{screeningId:guid}/seats", async (
            Guid screeningId,
            AppDbContext db) =>
        {
            var screening = await db.Screenings
                .Include(s => s.Hall)
                .FirstOrDefaultAsync(s => s.Id == screeningId);

            if (screening is null)
            {
                return Results.NotFound(new { message = "Screening not found." });
            }

            var now = DateTime.UtcNow;
            var reservations = await db.Reservations
                .Where(r => r.ScreeningId == screeningId)
                .Select(r => new
                {
                    r.SeatNo,
                    r.Status,
                    r.ExpiresAt
                })
                .ToListAsync();

            var activeSeats = reservations
                .Where(r => r.Status == ReservationStatus.CONFIRMED ||
                            (r.Status == ReservationStatus.PENDING_PAYMENT && r.ExpiresAt > now))
                .GroupBy(r => r.SeatNo)
                .ToDictionary(g => g.Key, g => g.First());

            var seats = Enumerable.Range(1, screening.Hall.NumberOfSeats)
                .Select(seatNo => new SeatAvailabilityDto(
                    seatNo,
                    activeSeats.ContainsKey(seatNo) ? "RESERVED" : "AVAILABLE"))
                .ToList();

            var response = new ScreeningSeatsResponse(
                screening.Id,
                new HallSummaryDto(screening.Hall.Id, screening.Hall.Title, screening.Hall.NumberOfSeats),
                seats);

            return Results.Ok(response);
        });
    }
}
