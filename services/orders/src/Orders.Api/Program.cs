using Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Orders.Api.Clients;
using Orders.Api.Config;
using Orders.Api.Data;
using Orders.Api.Data.Repositories;
using System.Text;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// -------------------- Services --------------------
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Orders API", Version = "v1" });

    // Add JWT Bearer auth to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste ONLY the JWT token here. Swagger will add 'Bearer ' automatically."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
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

// Repositories
builder.Services.AddScoped<CartRepository>();
builder.Services.AddScoped<OrderRepository>();

// Catalog HTTP Client
builder.Services.AddHttpClient<CatalogClient>(client =>
{
    client.BaseAddress = new Uri(Env.Require("CATALOG_SERVICE_URL"));
    client.Timeout = TimeSpan.FromSeconds(5);
});

// JWT Auth (validate only)
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
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// -------------------- Middleware --------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

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
app.MapGet("/api/orders/ping", () => Results.Ok(new { service = "orders", ok = true }));

app.MapControllers();
app.Run();
