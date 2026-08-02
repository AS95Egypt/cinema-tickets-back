namespace CinemaTicketsBack.Infrastructure.Database;

public class SqlServerConnectionFactory : IDatabaseConnectionFactory
{
    private readonly IConfiguration _configuration;

    public SqlServerConnectionFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetConnectionString()
    {
        return _configuration.GetConnectionString("DefaultConnection")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=CinemaTicketsDb;Trusted_Connection=True;TrustServerCertificate=True;";
    }
}
