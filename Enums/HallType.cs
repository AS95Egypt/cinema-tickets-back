using System.Text.Json.Serialization;

namespace CinemaTicketsBack.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HallType
{
    Standard,
    [JsonPropertyName("4D")]
    FourD,
    Gold,
    MAX,
    IMAX
}
