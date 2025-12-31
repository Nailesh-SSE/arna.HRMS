using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace arna.HRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addrelationshipwithemployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                columns: new[] { "CreatedAt", "Role", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 31, 10, 33, 39, 303, DateTimeKind.Utc).AddTicks(9934), 1, new DateTime(2025, 12, 31, 10, 33, 39, 303, DateTimeKind.Utc).AddTicks(9938) });

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmployeeId",
                table: "Users",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Employees_EmployeeId",
                table: "Users",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Employees_EmployeeId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_EmployeeId",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 30, 12, 6, 54, 450, DateTimeKind.Utc).AddTicks(2344), new DateTime(2025, 12, 30, 12, 6, 54, 450, DateTimeKind.Utc).AddTicks(2346) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 30, 12, 6, 54, 450, DateTimeKind.Utc).AddTicks(2350), new DateTime(2025, 12, 30, 12, 6, 54, 450, DateTimeKind.Utc).AddTicks(2350) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 30, 12, 6, 54, 450, DateTimeKind.Utc).AddTicks(2352), new DateTime(2025, 12, 30, 12, 6, 54, 450, DateTimeKind.Utc).AddTicks(2353) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 30, 12, 6, 54, 450, DateTimeKind.Utc).AddTicks(2355), new DateTime(2025, 12, 30, 12, 6, 54, 450, DateTimeKind.Utc).AddTicks(2355) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Role", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 30, 12, 6, 54, 449, DateTimeKind.Utc).AddTicks(8634), 0, new DateTime(2025, 12, 30, 12, 6, 54, 449, DateTimeKind.Utc).AddTicks(8636) });
        }
    }
}
