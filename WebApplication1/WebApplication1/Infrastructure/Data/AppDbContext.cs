using DocumentValidator.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocumentValidator.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Document> Documents { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Client> Clients { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(e =>
        {
            e.HasIndex(x => x.Email).IsUnique();
            e.HasIndex(x => x.EmployeeCode).IsUnique();
        });

        modelBuilder.Entity<Document>(e =>
        {
            e.HasOne(d => d.ValidatedByEmployee)
             .WithMany(emp => emp.ValidatedDocuments)
             .HasForeignKey(d => d.ValidatedByEmployeeId)
             .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
