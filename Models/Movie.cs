namespace CinemaTicketsBack.Models;

public class Movie
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public string? Description { get; set; }
    public DateTime ReleaseDate { get; set; }
}
