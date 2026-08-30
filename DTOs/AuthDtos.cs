namespace CinemaTicketsBack.DTOs;

public record RegisterRequest(string Username, string Email, string Password);

public record LoginRequest(string Email, string Password);

public record UserDto(Guid Id, string Username, string Email, bool IsAdmin);

public record AuthResponse(string AccessToken, int ExpiresIn, UserDto User);
