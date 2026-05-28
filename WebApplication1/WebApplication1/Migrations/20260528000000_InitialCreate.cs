using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Employees",
            columns: table => new
            {
                Id           = table.Column<int>(nullable: false).Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Name         = table.Column<string>(maxLength: 255, nullable: false),
                Email        = table.Column<string>(maxLength: 255, nullable: false),
                EmployeeCode = table.Column<string>(maxLength: 100, nullable: false),
                PasswordHash = table.Column<string>(maxLength: 255, nullable: false),
                Department   = table.Column<string>(maxLength: 100, nullable: false),
                Role         = table.Column<string>(maxLength: 50, nullable: false, defaultValue: "Analyst"),
                CreatedAt    = table.Column<DateTime>(nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
            },
            constraints: table => table.PrimaryKey("PK_Employees", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Clients",
            columns: table => new
            {
                Id            = table.Column<int>(nullable: false).Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Name          = table.Column<string>(maxLength: 255, nullable: false),
                Email         = table.Column<string>(maxLength: 255, nullable: false),
                DocumentType  = table.Column<string>(maxLength: 50, nullable: false),
                DocumentValue = table.Column<string>(maxLength: 255, nullable: false),
                Country       = table.Column<string>(maxLength: 100, nullable: false),
                CreatedAt     = table.Column<DateTime>(nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
            },
            constraints: table => table.PrimaryKey("PK_Clients", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Documents",
            columns: table => new
            {
                Id                    = table.Column<int>(nullable: false).Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                DocumentType          = table.Column<string>(maxLength: 50, nullable: false),
                DocumentValue         = table.Column<string>(maxLength: 255, nullable: false),
                IsValid               = table.Column<bool>(nullable: false),
                ConfidenceScore       = table.Column<int>(nullable: false),
                Country               = table.Column<string>(maxLength: 100, nullable: false),
                ValidatedAt           = table.Column<DateTime>(nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                ValidatedByEmployeeId = table.Column<int>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Documents", x => x.Id);
                table.ForeignKey(
                    name: "FK_Documents_Employees",
                    column: x => x.ValidatedByEmployeeId,
                    principalTable: "Employees",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(name: "IX_Employees_Email",        table: "Employees", column: "Email",        unique: true);
        migrationBuilder.CreateIndex(name: "IX_Employees_EmployeeCode", table: "Employees", column: "EmployeeCode", unique: true);
        migrationBuilder.CreateIndex(name: "IX_Documents_EmployeeId",   table: "Documents", column: "ValidatedByEmployeeId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Documents");
        migrationBuilder.DropTable(name: "Clients");
        migrationBuilder.DropTable(name: "Employees");
    }
}
