using Dapper;
using Identity.Api.Auth;
using Identity.Api.Config;
using Identity.Api.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// -------------------- Services --------------------
builder.Services.AddControllers();

builder.Services.AddSingleton<TokenService>();
builder.Services.AddSingleton<UserRepository>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Identity API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    c.AddSecurityRequirement(new()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


// DB options from environment variables
builder.Services.AddSingleton(new DbOptions
{
    Host = Env.Require("DB_HOST"),
    Name = Env.Require("DB_NAME"),
    User = Env.Require("DB_USER"),
    Password = Env.Require("DB_PASSWORD"),
});

builder.Services.AddSingleton<DapperContext>();

// JWT Auth
var jwtKey = Env.Require("JWT_SIGNING_KEY");
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),

            ValidateLifetime = true,
            RequireExpirationTime = true,

            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });


builder.Services.AddAuthorization();

var app = builder.Build();

Console.WriteLine("✅ Program.cs running");

using (var scope = app.Services.CreateScope())
{
    var repo = scope.ServiceProvider.GetService<UserRepository>();
    Console.WriteLine(repo is null
        ? "❌ UserRepository NOT resolved at startup"
        : "✅ UserRepository resolved at startup");
}


// -------------------- Middleware --------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/debug/jwt", () =>
{
    var k = Environment.GetEnvironmentVariable("JWT_SIGNING_KEY");
    return Results.Ok(new { hasKey = !string.IsNullOrWhiteSpace(k), len = k?.Length ?? 0 });
});


// -------------------- Health --------------------
app.MapGet("/health/live", () => Results.Ok(new { status = "live" }));

app.MapGet("/health/ready", async (DapperContext db) =>
{
    try
    {
        using var conn = db.CreateConnection();
        var ok = await conn.ExecuteScalarAsync<int>("SELECT 1;");
        return ok == 1 ? Results.Ok(new { status = "ready" }) : Results.StatusCode(503);
    }
    catch (Exception ex)
    {
        return Results.Problem(title: "DB not reachable", detail: ex.Message, statusCode: 503);
    }
});

// Ingress-prefix ping
app.MapGet("/api/auth/ping", () => Results.Ok(new { service = "identity", ok = true }));

app.MapControllers();
app.Run();
