using CinemaTicketsBack.Enums;

namespace CinemaTicketsBack.DTOs;

public record CreateReservationRequest(Guid ScreeningId, int SeatNo);

public record ReservationDto(
    Guid ReservationId,
    Guid ScreeningId,
    int SeatNo,
    ReservationStatus Status,
    DateTime ExpiresAt,
    decimal Amount,
    string Currency
);
