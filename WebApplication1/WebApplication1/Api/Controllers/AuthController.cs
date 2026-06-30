using DocumentValidator.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DocumentValidator.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;

    public AuthController(AuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var token = await _auth.LoginAsync(req.EmployeeCode, req.Password);
        if (token is null) return Unauthorized(new { message = "Invalid credentials." });
        return Ok(new { token });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var allowedRoles = new[] { "Admin", "Analyst", "Viewer" };
        var role = req.Role ?? "Analyst";
        if (!allowedRoles.Contains(role))
            return BadRequest(new { message = $"Role inválida. Permitidas: {string.Join(", ", allowedRoles)}." });

        if (req.Password.Length < 8 ||
            !req.Password.Any(char.IsUpper) ||
            !req.Password.Any(char.IsDigit))
            return BadRequest(new { message = "Senha deve ter no mínimo 8 caracteres, uma letra maiúscula e um número." });

        try
        {
            var employee = await _auth.RegisterAsync(
                req.Name, req.Email, req.EmployeeCode,
                req.Password, req.Department, role);

            return Ok(new { employee.Id, employee.Name, employee.EmployeeCode });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class LoginRequest
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string? Role { get; set; }
}
