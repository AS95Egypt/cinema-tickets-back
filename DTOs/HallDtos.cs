using CinemaTicketsBack.Enums;

namespace CinemaTicketsBack.DTOs;

public record CreateHallRequest(string Title, int NumberOfSeats, HallType Type);

public record UpdateHallRequest(string Title, int NumberOfSeats, HallType Type);

public record HallDto(
    Guid Id,
    string Title,
    int NumberOfSeats,
    HallType Type,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
