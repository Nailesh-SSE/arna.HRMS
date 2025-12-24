using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace arna.HRMS.Infrastructure.Migrations;

/// <inheritdoc />
public partial class SeedIdentityRoles : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.InsertData(
            table: "AspNetRoles",
            columns: new[] { "Id", "ConcurrencyStamp", "Description", "Name", "NormalizedName" },
            values: new object[,]
            {
                { 1, "6c5867b4-c54c-426d-a575-3a009b679f6d", "Administrator with full permissions", "Admin", "ADMIN" },
                { 2, "18fc92f0-4efd-4133-8a2e-7894855f8f8d", "Human Resources role", "HR", "HR" },
                { 3, "1bf2377b-5c26-4808-9d40-5d118adce3c7", "Manager role with team oversight", "Manager", "MANAGER" },
                { 4, "7acd2b57-9055-41ab-9cfe-87c2952b15ed", "Standard employee role", "Employee", "EMPLOYEE" }
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            table: "AspNetRoles",
            keyColumn: "Id",
            keyValue: 1);

        migrationBuilder.DeleteData(
            table: "AspNetRoles",
            keyColumn: "Id",
            keyValue: 2);

        migrationBuilder.DeleteData(
            table: "AspNetRoles",
            keyColumn: "Id",
            keyValue: 3);

        migrationBuilder.DeleteData(
            table: "AspNetRoles",
            keyColumn: "Id",
            keyValue: 4);
    }
}
