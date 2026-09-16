using System.Net;
using System.Net.Http.Json;
using CinemaTicketsBack.DTOs;
using CinemaTicketsBack.Tests.Infrastructure;

namespace CinemaTicketsBack.Tests.Endpoints;

[Collection(SqlServerCollection.Name)]
public class AuthEndpointsTests : IClassFixture<SqlServerContainerFixture>, IDisposable
{
    private readonly TestWebApplicationFactory _factory;

    public AuthEndpointsTests(SqlServerContainerFixture container)
    {
        _factory = new TestWebApplicationFactory(container);
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task Register_ValidUser_Returns201CreatedAndUserDto()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new RegisterRequest("ahmed", "ahmed@example.com", "Password123!");

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var userDto = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(userDto);
        Assert.Equal("ahmed", userDto.Username);
        Assert.Equal("ahmed@example.com", userDto.Email);
        Assert.False(userDto.IsAdmin);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns400BadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new RegisterRequest("user1", "duplicate@example.com", "Password123!");

        // First registration
        await client.PostAsJsonAsync("/api/v1/auth/register", request);

        // Second registration with same email
        var duplicateRequest = new RegisterRequest("user2", "duplicate@example.com", "Password456!");

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/register", duplicateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithAccessToken()
    {
        // Arrange
        var client = _factory.CreateClient();
        var email = "loginuser@example.com";
        var password = "Password123!";
        await client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest("loginuser", email, password));

        var loginRequest = new LoginRequest(email, password);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(authResponse);
        Assert.False(string.IsNullOrWhiteSpace(authResponse.AccessToken));
        Assert.True(authResponse.ExpiresIn > 0);
        Assert.Equal("loginuser", authResponse.User.Username);
    }

    [Fact]
    public async Task Login_InvalidPassword_Returns401Unauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        var email = "wrongpass@example.com";
        await client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest("wrongpassuser", email, "Password123!"));

        var loginRequest = new LoginRequest(email, "WrongPassword!");

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
