using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace arna.HRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addempIdUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmployeeId",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

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
                columns: new[] { "CreatedAt", "EmployeeId", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 30, 12, 6, 54, 449, DateTimeKind.Utc).AddTicks(8634), 0, new DateTime(2025, 12, 30, 12, 6, 54, 449, DateTimeKind.Utc).AddTicks(8636) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 24, 14, 0, 35, 507, DateTimeKind.Utc).AddTicks(469), new DateTime(2025, 12, 24, 14, 0, 35, 507, DateTimeKind.Utc).AddTicks(470) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 24, 14, 0, 35, 507, DateTimeKind.Utc).AddTicks(473), new DateTime(2025, 12, 24, 14, 0, 35, 507, DateTimeKind.Utc).AddTicks(474) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 24, 14, 0, 35, 507, DateTimeKind.Utc).AddTicks(475), new DateTime(2025, 12, 24, 14, 0, 35, 507, DateTimeKind.Utc).AddTicks(476) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 24, 14, 0, 35, 507, DateTimeKind.Utc).AddTicks(477), new DateTime(2025, 12, 24, 14, 0, 35, 507, DateTimeKind.Utc).AddTicks(477) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 12, 24, 14, 0, 35, 506, DateTimeKind.Utc).AddTicks(4449), new DateTime(2025, 12, 24, 14, 0, 35, 506, DateTimeKind.Utc).AddTicks(4451) });
        }
    }
}
