using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DocumentValidator.Domain.Entities;
using DocumentValidator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace DocumentValidator.Application.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<string?> LoginAsync(string employeeCode, string password)
    {
        var employee = await _db.Employees
            .FirstOrDefaultAsync(e => e.EmployeeCode == employeeCode);

        if (employee is null) return null;

        var hash = HashPassword(password);
        if (employee.PasswordHash != hash) return null;

        return GenerateToken(employee);
    }

    public async Task<Employee> RegisterAsync(string name, string email, string employeeCode, string password, string department, string role = "Analyst")
    {
        var employee = new Employee
        {
            Name = name,
            Email = email,
            EmployeeCode = employeeCode,
            PasswordHash = HashPassword(password),
            Department = department,
            Role = role
        };

        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();
        return employee;
    }

    public static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }

    private string GenerateToken(Employee employee)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not configured")));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString()),
            new Claim(ClaimTypes.Name, employee.Name),
            new Claim(ClaimTypes.Email, employee.Email),
            new Claim("EmployeeCode", employee.EmployeeCode),
            new Claim(ClaimTypes.Role, employee.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
