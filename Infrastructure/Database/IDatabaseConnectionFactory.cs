namespace CinemaTicketsBack.Infrastructure.Database;

public interface IDatabaseConnectionFactory
{
    string GetConnectionString();
}
