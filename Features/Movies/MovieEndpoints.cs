using CinemaTicketsBack.Models;

namespace CinemaTicketsBack.Features.Movies;

public static class MovieEndpoints
{
    public static void MapMovieEndpoints(this IEndpointRouteBuilder app)
    {
        var movies = new List<Movie>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Inception",
                Genre = "Sci-Fi",
                DurationMinutes = 148,
                Description = "A thief who enters dreams.",
                ReleaseDate = new DateTime(2010, 7, 16)
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "The Matrix",
                Genre = "Action",
                DurationMinutes = 136,
                Description = "A hacker discovers reality is a simulation.",
                ReleaseDate = new DateTime(1999, 3, 31)
            }
        };

        var group = app.MapGroup("/api/movies")
            .WithTags("Movies");

        group.MapGet("", () => Results.Ok(movies));

        group.MapGet("{id:guid}", (Guid id) =>
        {
            var movie = movies.FirstOrDefault(x => x.Id == id);
            return movie is null ? Results.NotFound(new { message = "Movie not found." }) : Results.Ok(movie);
        });

        group.MapPost("", (Movie movie) =>
        {
            movie.Id = Guid.NewGuid();
            movies.Add(movie);
            return Results.Created($"/api/movies/{movie.Id}", movie);
        });

        group.MapPut("{id:guid}", (Guid id, Movie updatedMovie) =>
        {
            var existingMovie = movies.FirstOrDefault(x => x.Id == id);
            if (existingMovie is null)
            {
                return Results.NotFound(new { message = "Movie not found." });
            }

            var index = movies.IndexOf(existingMovie);
            movies[index] = new Movie
            {
                Id = existingMovie.Id,
                Title = updatedMovie.Title,
                Genre = updatedMovie.Genre,
                DurationMinutes = updatedMovie.DurationMinutes,
                Description = updatedMovie.Description,
                ReleaseDate = updatedMovie.ReleaseDate
            };

            return Results.Ok(movies[index]);
        });

        group.MapDelete("{id:guid}", (Guid id) =>
        {
            var existingMovie = movies.FirstOrDefault(x => x.Id == id);
            if (existingMovie is null)
            {
                return Results.NotFound(new { message = "Movie not found." });
            }

            movies.Remove(existingMovie);
            return Results.NoContent();
        });
    }
}
