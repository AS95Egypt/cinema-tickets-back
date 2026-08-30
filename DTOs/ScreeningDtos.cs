using CinemaTicketsBack.Enums;

namespace CinemaTicketsBack.DTOs;

public record CreateScreeningRequest(
    Guid HallId,
    DateTime StartDateTime,
    decimal Price
);

public record ScreeningHallInfoDto(
    Guid Id,
    string Title,
    HallType Type
);

public record ScreeningDto(
    Guid Id,
    DateTime StartDateTime,
    decimal Price,
    ScreeningHallInfoDto Hall
);

public record MovieWithScreeningsDto(
    Guid MovieId,
    string Title,
    IReadOnlyList<ScreeningDto> Screenings
);

public record SeatAvailabilityDto(
    int SeatNo,
    string Status
);

public record HallSummaryDto(
    Guid Id,
    string Title,
    int NumberOfSeats
);

public record ScreeningSeatsResponse(
    Guid ScreeningId,
    HallSummaryDto Hall,
    IReadOnlyList<SeatAvailabilityDto> Seats
);
