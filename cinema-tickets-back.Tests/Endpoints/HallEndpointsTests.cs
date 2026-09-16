using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CinemaTicketsBack.DTOs;
using CinemaTicketsBack.Enums;
using CinemaTicketsBack.Infrastructure.Database;
using CinemaTicketsBack.Models;
using CinemaTicketsBack.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CinemaTicketsBack.Tests.Endpoints;

public class HallEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly string _dbName = $"HallTestDb_{Guid.NewGuid()}";

    public HallEndpointsTests(WebApplicationFactory<Program> factory)
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

    private string GetAdminToken()
    {
        using var scope = _factory.Services.CreateScope();
        var tokenGenerator = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();

        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "adminuser",
            Email = "admin@example.com",
            IsAdmin = true,
            IsActive = true
        };

        var (token, _) = tokenGenerator.GenerateToken(adminUser);
        return token;
    }

    private string GetNormalUserToken()
    {
        using var scope = _factory.Services.CreateScope();
        var tokenGenerator = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();

        var normalUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "normaluser",
            Email = "normal@example.com",
            IsAdmin = false,
            IsActive = true
        };

        var (token, _) = tokenGenerator.GenerateToken(normalUser);
        return token;
    }

    [Fact]
    public async Task CreateHall_AdminUser_Returns201Created()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetAdminToken());

        var request = new CreateHallRequest("IMAX Main Hall", 250, HallType.IMAX);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/halls", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var hallDto = await response.Content.ReadFromJsonAsync<HallDto>();
        Assert.NotNull(hallDto);
        Assert.Equal("IMAX Main Hall", hallDto.Title);
        Assert.Equal(250, hallDto.NumberOfSeats);
        Assert.Equal(HallType.IMAX, hallDto.Type);
        Assert.True(hallDto.IsActive);
    }

    [Fact]
    public async Task CreateHall_ZeroSeats_Returns400BadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetAdminToken());

        var request = new CreateHallRequest("Invalid Hall", 0, HallType.Standard);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/halls", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateHall_NonAdminUser_Returns403Forbidden()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetNormalUserToken());

        var request = new CreateHallRequest("Forbidden Hall", 100, HallType.Standard);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/halls", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetHalls_ReturnsListOfHalls_WithActiveFilter()
    {
        // Arrange
        var client = _factory.CreateClient();
        var adminToken = GetAdminToken();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var createResponse1 = await client.PostAsJsonAsync("/api/v1/halls", new CreateHallRequest("Active Hall", 100, HallType.Standard));
        var hall1 = await createResponse1.Content.ReadFromJsonAsync<HallDto>();

        var createResponse2 = await client.PostAsJsonAsync("/api/v1/halls", new CreateHallRequest("Deactivated Hall", 80, HallType.FourD));
        var hall2 = await createResponse2.Content.ReadFromJsonAsync<HallDto>();

        // Deactivate second hall
        await client.PatchAsync($"/api/v1/halls/{hall2!.Id}/deactivate", null);

        // Act - Fetch active only
        var activeOnlyResponse = await client.GetFromJsonAsync<List<HallDto>>("/api/v1/halls?activeOnly=true");

        // Assert
        Assert.NotNull(activeOnlyResponse);
        Assert.Contains(activeOnlyResponse, h => h.Id == hall1!.Id);
        Assert.DoesNotContain(activeOnlyResponse, h => h.Id == hall2.Id);
    }

    [Fact]
    public async Task UpdateHall_AdminUser_UpdatesHallDetails()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetAdminToken());

        var createResponse = await client.PostAsJsonAsync("/api/v1/halls", new CreateHallRequest("Old Title", 50, HallType.Standard));
        var hall = await createResponse.Content.ReadFromJsonAsync<HallDto>();

        var updateRequest = new UpdateHallRequest("Updated Gold Hall", 75, HallType.Gold);

        // Act
        var response = await client.PutAsJsonAsync($"/api/v1/halls/{hall!.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updatedHall = await response.Content.ReadFromJsonAsync<HallDto>();
        Assert.NotNull(updatedHall);
        Assert.Equal("Updated Gold Hall", updatedHall.Title);
        Assert.Equal(75, updatedHall.NumberOfSeats);
        Assert.Equal(HallType.Gold, updatedHall.Type);
        Assert.NotNull(updatedHall.UpdatedAt);
    }

    [Fact]
    public async Task DeactivateHall_AdminUser_SetsIsActiveToFalse()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetAdminToken());

        var createResponse = await client.PostAsJsonAsync("/api/v1/halls", new CreateHallRequest("Hall to Deactivate", 120, HallType.MAX));
        var hall = await createResponse.Content.ReadFromJsonAsync<HallDto>();

        // Act
        var response = await client.PatchAsync($"/api/v1/halls/{hall!.Id}/deactivate", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var deactivatedHall = await response.Content.ReadFromJsonAsync<HallDto>();
        Assert.NotNull(deactivatedHall);
        Assert.False(deactivatedHall.IsActive);
        Assert.NotNull(deactivatedHall.UpdatedAt);
    }
}
