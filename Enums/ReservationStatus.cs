using System.Text.Json.Serialization;

namespace CinemaTicketsBack.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReservationStatus
{
    PENDING_PAYMENT,
    CONFIRMED,
    CANCELLED,
    EXPIRED
}
