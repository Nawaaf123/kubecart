using BCrypt.Net;
using Identity.Api.Auth;
using Identity.Api.Data;
using Identity.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Identity.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly UserRepository _users;
    private readonly TokenService _tokens;

    public AuthController(UserRepository users, TokenService tokens)
    {
        _users = users;
        _tokens = tokens;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto req)
    {
        var exists = await _users.GetByEmailAsync(req.Email);
        if (exists != null) return BadRequest("User already exists.");

        var userId = Guid.NewGuid();
        var hash = BCrypt.Net.BCrypt.HashPassword(req.Password);

        var role = string.IsNullOrWhiteSpace(req.Role) ? "Customer" : req.Role.Trim();
        await _users.CreateAsync(userId, req.Email.Trim(), hash, role);

        return Ok(new { userId, role });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto req)
    {
        var user = await _users.GetByEmailAsync(req.Email.Trim());
        if (user == null) return Unauthorized("Invalid credentials.");

        if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials.");

        var token = _tokens.CreateToken(user.Id, user.Email, user.Role);
        return Ok(new { token });
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        var email =
            User.FindFirstValue(ClaimTypes.Email) ??
            User.FindFirstValue(JwtRegisteredClaimNames.Email);

        var role =
            User.FindFirstValue(ClaimTypes.Role) ??
            User.FindFirstValue("role");

        return Ok(new { userId, email, role });
    }

}
