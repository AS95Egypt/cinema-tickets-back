using System.Text.Json.Serialization;

namespace CinemaTicketsBack.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MovieGenre
{
    Comedy,
    Action,
    Drama,
    Fantasy
}
