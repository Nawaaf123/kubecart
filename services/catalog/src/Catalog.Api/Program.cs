using Catalog.Api.Config;
using Catalog.Api.Data;
using Catalog.Api.Data.Repositories;
using Dapper;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

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


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Catalog API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste ONLY the JWT token. Swagger adds 'Bearer ' automatically."
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

builder.Services.AddScoped<CategoryRepository>();
builder.Services.AddScoped<ProductRepository>();


builder.Services.AddSingleton(new DbOptions
{
    Host = Env.Require("DB_HOST"),
    Name = Env.Require("DB_NAME"),
    User = Env.Require("DB_USER"),
    Password = Env.Require("DB_PASSWORD")
});
builder.Services.AddSingleton<DapperContext>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseAuthentication();

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
        return Results.Problem(detail: ex.Message, statusCode: 503, title: "DB not reachable");
    }
});

app.MapGet("/api/catalog/ping", () => Results.Ok(new { service = "catalog", ok = true }));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapControllers();
app.Run();
