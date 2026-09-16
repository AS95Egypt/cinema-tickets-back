using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CinemaTicketsBack.Models;
using CinemaTicketsBack.Services;
using Microsoft.Extensions.Configuration;

namespace CinemaTicketsBack.Tests.Services;

public class JwtTokenGeneratorTests
{
    [Fact]
    public void GenerateToken_ReturnsValidSignedTokenWithUserClaims()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            {"JwtSettings:Secret", "SuperSecretKeyForCinemaTicketsApiJwtSigningMustBeAtLeast32BytesLong!"},
            {"JwtSettings:Issuer", "CinemaTicketsAPI"},
            {"JwtSettings:Audience", "CinemaTicketsClient"},
            {"JwtSettings:ExpiryMinutes", "60"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var generator = new JwtTokenGenerator(configuration);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "testuser@example.com",
            IsAdmin = true,
            IsActive = true
        };

        // Act
        var (token, expiresIn) = generator.GenerateToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.Equal(3600, expiresIn);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Assert.Equal("CinemaTicketsAPI", jwtToken.Issuer);
        Assert.Equal("CinemaTicketsClient", jwtToken.Audiences.First());

        var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub || c.Type == "sub")?.Value;
        var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email || c.Type == "email")?.Value;
        var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value;

        Assert.Equal(user.Id.ToString(), subClaim);
        Assert.Equal(user.Email, emailClaim);
        Assert.Equal("Admin", roleClaim);
    }
}
