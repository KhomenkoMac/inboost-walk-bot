using Microsoft.Data.SqlClient;
using System.Data;

namespace api.Shared.DB;

internal class AppDbContext : IDisposable
{
    private readonly string connectionString;
    private SqlConnection connection;

    public AppDbContext(IConfiguration configuration)
    {
        connectionString = configuration.GetConnectionString("Default");
    }

    public IDbConnection CreateConnection()
    {
        connection = new SqlConnection(connectionString);
        return connection;
    }

    public void Dispose()
    {
        connection.Dispose();
    }
}
