using CinemaTicketsBack.Features.Movies;

namespace CinemaTicketsBack.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapApplicationEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapMovieEndpoints();
        return app;
    }
}
