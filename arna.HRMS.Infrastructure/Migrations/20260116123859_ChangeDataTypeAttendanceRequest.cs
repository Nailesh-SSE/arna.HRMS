using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace arna.HRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeDataTypeAttendanceRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ReasonType",
                table: "AttendanceRequest",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "AttendanceRequest",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 16, 18, 8, 57, 266, DateTimeKind.Local).AddTicks(747), new DateTime(2026, 1, 16, 18, 8, 57, 266, DateTimeKind.Local).AddTicks(747) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 16, 18, 8, 57, 266, DateTimeKind.Local).AddTicks(750), new DateTime(2026, 1, 16, 18, 8, 57, 266, DateTimeKind.Local).AddTicks(751) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 16, 18, 8, 57, 266, DateTimeKind.Local).AddTicks(752), new DateTime(2026, 1, 16, 18, 8, 57, 266, DateTimeKind.Local).AddTicks(752) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 16, 18, 8, 57, 266, DateTimeKind.Local).AddTicks(754), new DateTime(2026, 1, 16, 18, 8, 57, 266, DateTimeKind.Local).AddTicks(754) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 16, 18, 8, 57, 265, DateTimeKind.Local).AddTicks(9486), new DateTime(2026, 1, 16, 18, 8, 57, 265, DateTimeKind.Local).AddTicks(9498) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ReasonType",
                table: "AttendanceRequest",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "Location",
                table: "AttendanceRequest",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 16, 16, 33, 22, 833, DateTimeKind.Local).AddTicks(2600), new DateTime(2026, 1, 16, 16, 33, 22, 833, DateTimeKind.Local).AddTicks(2600) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 16, 16, 33, 22, 833, DateTimeKind.Local).AddTicks(2603), new DateTime(2026, 1, 16, 16, 33, 22, 833, DateTimeKind.Local).AddTicks(2603) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 16, 16, 33, 22, 833, DateTimeKind.Local).AddTicks(2605), new DateTime(2026, 1, 16, 16, 33, 22, 833, DateTimeKind.Local).AddTicks(2605) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 16, 16, 33, 22, 833, DateTimeKind.Local).AddTicks(2606), new DateTime(2026, 1, 16, 16, 33, 22, 833, DateTimeKind.Local).AddTicks(2606) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 16, 16, 33, 22, 833, DateTimeKind.Local).AddTicks(1699), new DateTime(2026, 1, 16, 16, 33, 22, 833, DateTimeKind.Local).AddTicks(1709) });
        }
    }
}
