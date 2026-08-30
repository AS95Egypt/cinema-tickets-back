using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CinemaTicketsBack.DTOs;
using CinemaTicketsBack.Enums;
using CinemaTicketsBack.Infrastructure.Database;
using CinemaTicketsBack.Models;
using CinemaTicketsBack.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CinemaTicketsBack.Features.Reservations;

public static class ReservationEndpoints
{
    public static void MapReservationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/reservations").WithTags("Reservations");

        group.MapPost("", async (
            CreateReservationRequest request,
            AppDbContext db,
            ClaimsPrincipal user,
            IOptions<ReservationSettings> reservationOptions) =>
        {
            if (!TryGetUserId(user, out var userId))
            {
                return Results.Json(new { message = "Invalid authentication token." }, statusCode: 401);
            }

            var settings = reservationOptions.Value;
            if (settings.HoldDurationMinutes <= 0)
            {
                throw new InvalidOperationException("ReservationSettings:HoldDurationMinutes must be greater than zero.");
            }

            var screening = await db.Screenings
                .Include(s => s.Hall)
                .Include(s => s.Movie)
                .FirstOrDefaultAsync(s => s.Id == request.ScreeningId);

            if (screening is null)
            {
                return Results.NotFound(new { message = "Screening not found." });
            }

            if (!screening.Movie.IsActive)
            {
                return Results.BadRequest(new { message = "Cannot reserve a seat for an inactive movie." });
            }

            if (!screening.Hall.IsActive)
            {
                return Results.BadRequest(new { message = "Cannot reserve a seat in an inactive hall." });
            }

            if (screening.StartDateTime <= DateTime.UtcNow)
            {
                return Results.BadRequest(new { message = "Cannot reserve a seat for a screening that has already started." });
            }

            if (!IsValidSeatNo(request.SeatNo, screening.Hall.NumberOfSeats))
            {
                return Results.BadRequest(new { message = "Seat number is invalid for this hall." });
            }

            await using var tx = await db.Database.BeginTransactionAsync();

            try
            {
                var now = DateTime.UtcNow;
                var stale = await db.Reservations
                    .Where(r => r.ScreeningId == request.ScreeningId
                             && r.SeatNo == request.SeatNo
                             && r.Status == ReservationStatus.PENDING_PAYMENT
                             && r.ExpiresAt <= now)
                    .ToListAsync();

                foreach (var row in stale)
                {
                    row.Status = ReservationStatus.EXPIRED;
                    row.UpdatedAt = now;
                }

                // TODO make sure seat is not already reserved by another user for this screening, return 409 conflict

                var reservation = new Reservation
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ScreeningId = screening.Id,
                    SeatNo = request.SeatNo,
                    Status = ReservationStatus.PENDING_PAYMENT,
                    CreatedAt = now,
                    ExpiresAt = now.AddMinutes(settings.HoldDurationMinutes),
                    Amount = screening.Price,
                    Currency = settings.Currency
                };

                db.Reservations.Add(reservation);
                await db.SaveChangesAsync();
                await tx.CommitAsync();

                var dto = new ReservationDto(
                    reservation.Id,
                    reservation.ScreeningId,
                    reservation.SeatNo,
                    reservation.Status,
                    reservation.ExpiresAt,
                    reservation.Amount,
                    reservation.Currency);

                return Results.Created($"/api/v1/reservations/{reservation.Id}", dto);
            }
            catch (DbUpdateException)
            {
                await tx.RollbackAsync();
                return Results.Json(new { message = "This seat is already reserved for this screening." }, statusCode: 409);
            }
        }).RequireAuthorization();
    }

    internal static bool IsValidSeatNo(int seatNo, int hallCapacity)
    {
        return seatNo >= 1 && seatNo <= hallCapacity;
    }

    private static bool TryGetUserId(ClaimsPrincipal user, out Guid userId)
    {
        var raw = user.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? user.FindFirstValue("sub");

        return Guid.TryParse(raw, out userId);
    }
}
