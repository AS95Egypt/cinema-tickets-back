using CinemaTicketsBack.Enums;

namespace CinemaTicketsBack.Models;

public class Hall
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public int NumberOfSeats { get; set; }
    public HallType Type { get; set; } = HallType.Standard;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
