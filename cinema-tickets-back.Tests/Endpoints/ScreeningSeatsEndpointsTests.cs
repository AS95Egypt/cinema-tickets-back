using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CinemaTicketsBack.DTOs;
using CinemaTicketsBack.Enums;
using CinemaTicketsBack.Infrastructure.Database;
using CinemaTicketsBack.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CinemaTicketsBack.Tests.Endpoints;

public class ScreeningSeatsEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly string _dbName = $"ScreeningSeatsTestDb_{Guid.NewGuid()}";

    [Fact]
    public async Task GetAllScreenings_FiltersByMovieAndHallAndReturnsPaginatedDescendingResults()
    {
        var client = _factory.CreateClient();

        Guid movieId;
        Guid hallId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var hall1 = new Hall
            {
                Id = Guid.NewGuid(),
                Title = "Main Hall",
                NumberOfSeats = 10,
                Type = HallType.Standard,
                IsActive = true
            };

            var hall2 = new Hall
            {
                Id = Guid.NewGuid(),
                Title = "VIP Hall",
                NumberOfSeats = 8,
                Type = HallType.Gold,
                IsActive = true
            };

            var movie1 = new Movie
            {
                Id = Guid.NewGuid(),
                Title = "Movie One",
                Genre = MovieGenre.Action,
                Duration = 120,
                ReleaseDate = DateTime.UtcNow.AddDays(-30),
                Language = "English",
                IsActive = true
            };

            var movie2 = new Movie
            {
                Id = Guid.NewGuid(),
                Title = "Movie Two",
                Genre = MovieGenre.Drama,
                Duration = 90,
                ReleaseDate = DateTime.UtcNow.AddDays(-10),
                Language = "English",
                IsActive = true
            };

            var oldScreening = new Screening
            {
                Id = Guid.NewGuid(),
                MovieId = movie1.Id,
                HallId = hall1.Id,
                StartDateTime = DateTime.UtcNow.AddDays(2),
                Price = 120m,
            };

            var newScreening = new Screening
            {
                Id = Guid.NewGuid(),
                MovieId = movie1.Id,
                HallId = hall1.Id,
                StartDateTime = DateTime.UtcNow.AddDays(5),
                Price = 150m,
            };

            var otherMovieScreening = new Screening
            {
                Id = Guid.NewGuid(),
                MovieId = movie2.Id,
                HallId = hall2.Id,
                StartDateTime = DateTime.UtcNow.AddDays(3),
                Price = 200m,
            };

            db.Halls.AddRange(hall1, hall2);
            db.Movies.AddRange(movie1, movie2);
            db.Screenings.AddRange(oldScreening, newScreening, otherMovieScreening);
            await db.SaveChangesAsync();

            movieId = movie1.Id;
            hallId = hall1.Id;
        }

        var response = await client.GetAsync($"/api/v1/screenings/all?movieId={movieId}&hallId={hallId}&page=1&pageSize=2");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = json.RootElement;

        Assert.Equal(2, root.GetProperty("totalCount").GetInt32());
        Assert.Equal(1, root.GetProperty("page").GetInt32());
        Assert.Equal(2, root.GetProperty("pageSize").GetInt32());

        var items = root.GetProperty("items").EnumerateArray().ToList();
        Assert.Equal(2, items.Count);

        var first = DateTime.Parse(items[0].GetProperty("startDateTime").GetString()!);
        var second = DateTime.Parse(items[1].GetProperty("startDateTime").GetString()!);
        Assert.True(first > second);
        Assert.Equal(movieId, Guid.Parse(items[0].GetProperty("movie").GetProperty("id").GetString()!));
        Assert.Equal(hallId, Guid.Parse(items[0].GetProperty("hall").GetProperty("id").GetString()!));
    }

    public ScreeningSeatsEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_dbName);
                });
            });
        });
    }

    [Fact]
    public async Task GetScreeningSeats_ReturnsReservedAndAvailableSeats()
    {
        var client = _factory.CreateClient();
        Guid screeningId;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var hall = new Hall
            {
                Id = Guid.NewGuid(),
                Title = "Main Hall",
                NumberOfSeats = 3,
                Type = HallType.Standard
            };

            var movie = new Movie
            {
                Id = Guid.NewGuid(),
                Title = "Sample Movie",
                Genre = MovieGenre.Action,
                Duration = 120,
                ReleaseDate = DateTime.UtcNow.AddDays(10),
                Language = "English"
            };

            var screening = new Screening
            {
                Id = Guid.NewGuid(),
                MovieId = movie.Id,
                HallId = hall.Id,
                StartDateTime = DateTime.UtcNow.AddDays(2),
                Price = 150m
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "tester",
                Email = "tester@example.com",
                PasswordHash = "hash",
                IsAdmin = false,
                IsActive = true
            };

            var confirmedReservation = new Reservation
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                ScreeningId = screening.Id,
                SeatNo = 2,
                Status = ReservationStatus.CONFIRMED,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                Amount = 150m,
                Currency = "EGP"
            };

            db.Halls.Add(hall);
            db.Movies.Add(movie);
            db.Users.Add(user);
            db.Screenings.Add(screening);
            db.Reservations.Add(confirmedReservation);
            await db.SaveChangesAsync();

            screeningId = screening.Id;
        }

        var response = await client.GetAsync($"/api/v1/screenings/{screeningId}/seats");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var seatsResponse = await response.Content.ReadFromJsonAsync<ScreeningSeatsResponse>();
        Assert.NotNull(seatsResponse);
        Assert.Equal(screeningId, seatsResponse.ScreeningId);
        Assert.Equal(3, seatsResponse.Hall.NumberOfSeats);
        Assert.Equal(new[] { 1, 2, 3 }, seatsResponse.Seats.Select(s => s.SeatNo).ToArray());
        Assert.Equal("AVAILABLE", seatsResponse.Seats[0].Status);
        Assert.Equal("RESERVED", seatsResponse.Seats[1].Status);
        Assert.Equal("AVAILABLE", seatsResponse.Seats[2].Status);
    }
}
