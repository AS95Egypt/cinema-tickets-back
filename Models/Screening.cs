namespace CinemaTicketsBack.Models;

public class Screening
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MovieId { get; set; }
    public Guid HallId { get; set; }
    public DateTime StartDateTime { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Movie Movie { get; set; } = null!;
    public Hall Hall { get; set; } = null!;
}
