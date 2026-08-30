using CinemaTicketsBack.Enums;

namespace CinemaTicketsBack.DTOs;

public record CreateMovieRequest(
    string Title,
    MovieGenre Genre,
    int Duration,
    DateTime ReleaseDate,
    string Language,
    string? Description,
    string? Actors,
    string? TrailerUrl
);

public record UpdateMovieRequest(
    string Title,
    MovieGenre Genre,
    int Duration,
    DateTime ReleaseDate,
    string Language,
    string? Description,
    string? Actors,
    string? TrailerUrl
);

public record MovieDto(
    Guid Id,
    string Title,
    MovieGenre Genre,
    int Duration,
    DateTime ReleaseDate,
    string Language,
    string? Description,
    string? Actors,
    string? TrailerUrl,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record MovieSummaryDto(
    Guid Id,
    string Title,
    MovieGenre Genre,
    int Duration,
    DateTime ReleaseDate,
    string Language
);
