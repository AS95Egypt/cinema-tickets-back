namespace CinemaTicketsBack.Services;

public class ReservationSettings
{
    public const string SectionName = "ReservationSettings";
    public int HoldDurationMinutes { get; set; } = 15;
    public string Currency { get; set; } = "EGP";
}
