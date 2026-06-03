using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DocumentValidator.Infrastructure.Data;

// Used only by EF Core CLI tools (dotnet ef migrations) — not loaded at runtime.
// Connection string is read from the DB_CONNECTION_STRING environment variable,
// or falls back to a local dev default (never hardcode credentials here).
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
            ?? "Server=localhost;Port=3306;Database=document_validation;User=root;Password=YOUR_LOCAL_DEV_PASSWORD;";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0)))
            .Options;

        return new AppDbContext(options);
    }
}
