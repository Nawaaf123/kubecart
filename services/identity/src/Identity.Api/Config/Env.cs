namespace Identity.Api.Config;

public static class Env
{
    public static string Require(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Missing required environment variable: {name}");
        return value;
    }

    public static string Optional(string name, string? defaultValue = null)
        => Environment.GetEnvironmentVariable(name) ?? defaultValue ?? string.Empty;
}
