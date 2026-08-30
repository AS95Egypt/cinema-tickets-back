using CinemaTicketsBack.Enums;

namespace CinemaTicketsBack.Models;

public class Movie
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public MovieGenre Genre { get; set; } = MovieGenre.Action;
    public int Duration { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string Language { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Actors { get; set; }
    public string? TrailerUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
