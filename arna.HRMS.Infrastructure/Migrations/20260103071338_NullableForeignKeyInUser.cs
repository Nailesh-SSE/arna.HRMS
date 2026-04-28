using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace arna.HRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NullableForeignKeyInUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Employees_EmployeeId",
                table: "Users");

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeId",
                table: "Users",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 7, 13, 38, 519, DateTimeKind.Utc).AddTicks(4704), new DateTime(2026, 1, 3, 7, 13, 38, 519, DateTimeKind.Utc).AddTicks(4706) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 7, 13, 38, 519, DateTimeKind.Utc).AddTicks(4709), new DateTime(2026, 1, 3, 7, 13, 38, 519, DateTimeKind.Utc).AddTicks(4710) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 7, 13, 38, 519, DateTimeKind.Utc).AddTicks(4711), new DateTime(2026, 1, 3, 7, 13, 38, 519, DateTimeKind.Utc).AddTicks(4711) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 7, 13, 38, 519, DateTimeKind.Utc).AddTicks(4712), new DateTime(2026, 1, 3, 7, 13, 38, 519, DateTimeKind.Utc).AddTicks(4713) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "EmployeeId", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 7, 13, 38, 518, DateTimeKind.Utc).AddTicks(7972), null, new DateTime(2026, 1, 3, 7, 13, 38, 518, DateTimeKind.Utc).AddTicks(7974) });

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Employees_EmployeeId",
                table: "Users",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Employees_EmployeeId",
                table: "Users");

            migrationBuilder.AlterColumn<int>(
                name: "EmployeeId",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

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
                columns: new[] { "CreatedAt", "EmployeeId", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 31, 10, 33, 39, 303, DateTimeKind.Utc).AddTicks(9934), 0, new DateTime(2025, 12, 31, 10, 33, 39, 303, DateTimeKind.Utc).AddTicks(9938) });

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Employees_EmployeeId",
                table: "Users",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
