namespace Orders.Api.Data;

public sealed class DbOptions
{
    public required string Host { get; init; }
    public required string Name { get; init; }
    public required string User { get; init; }
    public required string Password { get; init; }
}
