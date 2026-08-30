using CinemaTicketsBack.Enums;

namespace CinemaTicketsBack.Models;

public class Reservation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid ScreeningId { get; set; }
    public int SeatNo { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.PENDING_PAYMENT;
    public DateTime ExpiresAt { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EGP";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public Screening Screening { get; set; } = null!;
}
