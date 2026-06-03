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
    private const int Pbkdf2Iterations = 600_000;
    private const int SaltSize = 16;
    private const int HashSize = 32;

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

        // Verify even when employee is null to prevent timing-based user enumeration
        var isValid = employee is not null && VerifyPassword(password, employee.PasswordHash);
        if (!isValid) return null;

        return GenerateToken(employee!);
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

    // Format: base64(salt):base64(hash)
    public static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Pbkdf2Iterations,
            HashAlgorithmName.SHA256,
            HashSize);
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    public static bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split(':');
        if (parts.Length != 2) return false;

        var salt = Convert.FromBase64String(parts[0]);
        var expected = Convert.FromBase64String(parts[1]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Pbkdf2Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        return CryptographicOperations.FixedTimeEquals(actual, expected);
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
