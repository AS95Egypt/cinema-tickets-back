using CinemaTicketsBack.Services;

namespace CinemaTicketsBack.Tests.Services;

public class PasswordHasherTests
{
    private readonly PasswordHasher _passwordHasher = new();

    [Fact]
    public void HashPassword_ProducesValidBCryptHashAndVerifies()
    {
        // Arrange
        var password = "Password123!";

        // Act
        var hash = _passwordHasher.HashPassword(password);
        var isValid = _passwordHasher.VerifyPassword(password, hash);
        var isInvalid = _passwordHasher.VerifyPassword("WrongPassword", hash);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEqual(password, hash);
        Assert.True(isValid);
        Assert.False(isInvalid);
    }
}
