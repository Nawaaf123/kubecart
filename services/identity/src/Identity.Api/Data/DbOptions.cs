namespace Identity.Api.Data;

public sealed class DbOptions
{
    public required string Host { get; init; }     // DB_HOST
    public required string Name { get; init; }     // DB_NAME
    public required string User { get; init; }     // DB_USER
    public required string Password { get; init; } // DB_PASSWORD
}
