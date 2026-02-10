using Dapper;
using Identity.Api.Data;
using Identity.Api.Models;

namespace Identity.Api.Data;

public sealed class UserRepository
{
    private readonly DapperContext _db;

    public UserRepository(DapperContext db)
    {
        _db = db;
    }

    public async Task<UserRow?> GetByEmailAsync(string email)
    {
        const string sql = """
            SELECT TOP 1 Id, Email, PasswordHash, Role
            FROM dbo.Users
            WHERE Email = @Email;
        """;

        using var conn = _db.CreateConnection();
        return await conn.QuerySingleOrDefaultAsync<UserRow>(sql, new { Email = email });
    }

    public async Task CreateAsync(Guid id, string email, string passwordHash, string role)
    {
        const string sql = """
            INSERT INTO dbo.Users (Id, Email, PasswordHash, Role, CreatedAtUtc)
            VALUES (@Id, @Email, @PasswordHash, @Role, SYSUTCDATETIME());
        """;

        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(sql, new
        {
            Id = id,
            Email = email,
            PasswordHash = passwordHash,
            Role = role
        });
    }
}
