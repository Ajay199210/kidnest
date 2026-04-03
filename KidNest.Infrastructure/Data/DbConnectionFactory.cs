using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

public class DbConnectionFactory
{
    private static string? _connectionString;

    public static void Initialize(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("KidNestDbConnection")!;
    }

    public static SqlConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
