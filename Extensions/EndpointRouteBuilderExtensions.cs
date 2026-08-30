using CinemaTicketsBack.Features.Auth;
using CinemaTicketsBack.Features.Halls;
using CinemaTicketsBack.Features.Movies;
using CinemaTicketsBack.Features.Reservations;
using CinemaTicketsBack.Features.Screenings;

namespace CinemaTicketsBack.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapApplicationEndpoints(this IEndpointRouteBuilder app)
    {
        var apiV1 = app.MapGroup("/api/v1");

        apiV1.MapMovieEndpoints();
        apiV1.MapAuthEndpoints();
        apiV1.MapHallEndpoints();
        apiV1.MapScreeningEndpoints();
        apiV1.MapReservationEndpoints();
        apiV1.MapHealthChecks("/health");

        return app;
    }
}
