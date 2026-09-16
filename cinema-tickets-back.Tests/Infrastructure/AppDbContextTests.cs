using CinemaTicketsBack.Infrastructure.Database;
using CinemaTicketsBack.Models;
using Microsoft.EntityFrameworkCore;

namespace CinemaTicketsBack.Tests.Infrastructure;

public class AppDbContextTests
{
    [Fact]
    public async Task CanAddAndRetrieveMovie()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var movie = new Movie
        {
            Id = Guid.NewGuid(),
            Title = "Interstellar",
            Genre = CinemaTicketsBack.Enums.MovieGenre.Action,
            Duration = 169,
            Language = "English",
            Description = "A team of explorers travel through a wormhole in space.",
            ReleaseDate = new DateTime(2014, 11, 7)
        };

        // Act
        using (var context = new AppDbContext(options))
        {
            context.Movies.Add(movie);
            await context.SaveChangesAsync();
        }

        // Assert
        using (var context = new AppDbContext(options))
        {
            var savedMovie = await context.Movies.FirstOrDefaultAsync(m => m.Id == movie.Id);
            Assert.NotNull(savedMovie);
            Assert.Equal("Interstellar", savedMovie.Title);
            Assert.Equal(CinemaTicketsBack.Enums.MovieGenre.Action, savedMovie.Genre);
            Assert.Equal(169, savedMovie.Duration);
        }
    }
}
