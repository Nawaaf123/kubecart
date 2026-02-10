using System.Data;
using Microsoft.Data.SqlClient;

namespace Catalog.Api.Data;

public sealed class DapperContext
{
    private readonly string _connectionString;

    public DapperContext(DbOptions options)
    {
        _connectionString =
            $"Server={options.Host};" +
            $"Database={options.Name};" +
            $"User Id={options.User};" +
            $"Password={options.Password};" +
            $"Trusted_Connection=False;" +
            $"TrustServerCertificate=True;" +
            $"Encrypt=False;" +
            $"MultipleActiveResultSets=True;" +
            $"Connection Timeout=5;";
    }

    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}
