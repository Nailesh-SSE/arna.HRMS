using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace arna.HRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ResolveMigrationIssue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Employees",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 13, 9, 17, 943, DateTimeKind.Utc).AddTicks(238), new DateTime(2026, 1, 2, 13, 9, 17, 943, DateTimeKind.Utc).AddTicks(239) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 13, 9, 17, 943, DateTimeKind.Utc).AddTicks(241), new DateTime(2026, 1, 2, 13, 9, 17, 943, DateTimeKind.Utc).AddTicks(241) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 13, 9, 17, 943, DateTimeKind.Utc).AddTicks(242), new DateTime(2026, 1, 2, 13, 9, 17, 943, DateTimeKind.Utc).AddTicks(243) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 13, 9, 17, 943, DateTimeKind.Utc).AddTicks(244), new DateTime(2026, 1, 2, 13, 9, 17, 943, DateTimeKind.Utc).AddTicks(244) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 2, 13, 9, 17, 942, DateTimeKind.Utc).AddTicks(9593), new DateTime(2026, 1, 2, 13, 9, 17, 942, DateTimeKind.Utc).AddTicks(9595) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 31, 10, 33, 39, 304, DateTimeKind.Utc).AddTicks(3547), new DateTime(2025, 12, 31, 10, 33, 39, 304, DateTimeKind.Utc).AddTicks(3549) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 31, 10, 33, 39, 304, DateTimeKind.Utc).AddTicks(3552), new DateTime(2025, 12, 31, 10, 33, 39, 304, DateTimeKind.Utc).AddTicks(3553) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 31, 10, 33, 39, 304, DateTimeKind.Utc).AddTicks(3554), new DateTime(2025, 12, 31, 10, 33, 39, 304, DateTimeKind.Utc).AddTicks(3555) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 31, 10, 33, 39, 304, DateTimeKind.Utc).AddTicks(3556), new DateTime(2025, 12, 31, 10, 33, 39, 304, DateTimeKind.Utc).AddTicks(3556) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 31, 10, 33, 39, 303, DateTimeKind.Utc).AddTicks(9934), new DateTime(2025, 12, 31, 10, 33, 39, 303, DateTimeKind.Utc).AddTicks(9938) });
        }
    }
}
