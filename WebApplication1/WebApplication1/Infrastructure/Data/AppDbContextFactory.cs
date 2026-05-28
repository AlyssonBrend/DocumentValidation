using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DocumentValidator.Infrastructure.Data;

// Used only by EF Core CLI tools (dotnet ef migrations) — not loaded at runtime.
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(
                "Server=localhost;Port=3306;Database=document_validation;User=root;Password=root;",
                new MySqlServerVersion(new Version(8, 0, 0)))
            .Options;

        return new AppDbContext(options);
    }
}
