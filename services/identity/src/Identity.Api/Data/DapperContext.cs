using System.Data;
using Microsoft.Data.SqlClient;

namespace Identity.Api.Data;

public sealed class DapperContext
{
    private readonly string _connectionString;

    public DapperContext(DbOptions options)
    {
        // SQL Server Authentication (username/password)
        // Local dev: TrustServerCertificate=True avoids certificate issues.
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

    public IDbConnection CreateConnection()
        => new SqlConnection(_connectionString);
}
