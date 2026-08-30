using CinemaTicketsBack.Models;

namespace CinemaTicketsBack.Services;

public interface IJwtTokenGenerator
{
    (string Token, int ExpiresIn) GenerateToken(User user);
}
