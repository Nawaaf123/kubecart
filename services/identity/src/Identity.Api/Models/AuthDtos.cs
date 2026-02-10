namespace Identity.Api.Models;

public sealed record RegisterDto(string Email, string Password, string Role);
public sealed record LoginDto(string Email, string Password);
