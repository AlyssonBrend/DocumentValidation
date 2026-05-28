using DocumentValidator.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace WebApplication1.Migrations;

[DbContext(typeof(AppDbContext))]
partial class AppDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.2")
            .HasAnnotation("Relational:MaxIdentifierLength", 64);

        modelBuilder.Entity("DocumentValidator.Domain.Entities.Client", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
            b.Property<string>("Country").IsRequired().HasMaxLength(100).HasColumnType("varchar(100)");
            b.Property<DateTime>("CreatedAt").HasColumnType("datetime(6)");
            b.Property<string>("DocumentType").IsRequired().HasMaxLength(50).HasColumnType("varchar(50)");
            b.Property<string>("DocumentValue").IsRequired().HasMaxLength(255).HasColumnType("varchar(255)");
            b.Property<string>("Email").IsRequired().HasMaxLength(255).HasColumnType("varchar(255)");
            b.Property<string>("Name").IsRequired().HasMaxLength(255).HasColumnType("varchar(255)");
            b.HasKey("Id");
            b.ToTable("Clients");
        });

        modelBuilder.Entity("DocumentValidator.Domain.Entities.Document", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
            b.Property<int>("ConfidenceScore").HasColumnType("int");
            b.Property<string>("Country").IsRequired().HasMaxLength(100).HasColumnType("varchar(100)");
            b.Property<string>("DocumentType").IsRequired().HasMaxLength(50).HasColumnType("varchar(50)");
            b.Property<string>("DocumentValue").IsRequired().HasMaxLength(255).HasColumnType("varchar(255)");
            b.Property<bool>("IsValid").HasColumnType("tinyint(1)");
            b.Property<DateTime>("ValidatedAt").HasColumnType("datetime(6)");
            b.Property<int?>("ValidatedByEmployeeId").HasColumnType("int");
            b.HasKey("Id");
            b.HasIndex("ValidatedByEmployeeId");
            b.ToTable("Documents");
        });

        modelBuilder.Entity("DocumentValidator.Domain.Entities.Employee", b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
            b.Property<DateTime>("CreatedAt").HasColumnType("datetime(6)");
            b.Property<string>("Department").IsRequired().HasMaxLength(100).HasColumnType("varchar(100)");
            b.Property<string>("Email").IsRequired().HasMaxLength(255).HasColumnType("varchar(255)");
            b.Property<string>("EmployeeCode").IsRequired().HasMaxLength(100).HasColumnType("varchar(100)");
            b.Property<string>("Name").IsRequired().HasMaxLength(255).HasColumnType("varchar(255)");
            b.Property<string>("PasswordHash").IsRequired().HasMaxLength(255).HasColumnType("varchar(255)");
            b.Property<string>("Role").IsRequired().HasMaxLength(50).HasColumnType("varchar(50)");
            b.HasKey("Id");
            b.HasIndex("Email").IsUnique();
            b.HasIndex("EmployeeCode").IsUnique();
            b.ToTable("Employees");
        });

        modelBuilder.Entity("DocumentValidator.Domain.Entities.Document", b =>
        {
            b.HasOne("DocumentValidator.Domain.Entities.Employee", "ValidatedByEmployee")
             .WithMany("ValidatedDocuments")
             .HasForeignKey("ValidatedByEmployeeId")
             .OnDelete(DeleteBehavior.SetNull);
            b.Navigation("ValidatedByEmployee");
        });

        modelBuilder.Entity("DocumentValidator.Domain.Entities.Employee", b =>
        {
            b.Navigation("ValidatedDocuments");
        });
#pragma warning restore 612, 618
    }
}
