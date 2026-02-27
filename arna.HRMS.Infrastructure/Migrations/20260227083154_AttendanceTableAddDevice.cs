using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace arna.HRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AttendanceTableAddDevice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Device",
                table: "Attendances",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 27, 14, 1, 52, 470, DateTimeKind.Local).AddTicks(3620), new DateTime(2026, 2, 27, 14, 1, 52, 470, DateTimeKind.Local).AddTicks(3620) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 27, 14, 1, 52, 470, DateTimeKind.Local).AddTicks(3623), new DateTime(2026, 2, 27, 14, 1, 52, 470, DateTimeKind.Local).AddTicks(3623) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 27, 14, 1, 52, 470, DateTimeKind.Local).AddTicks(3624), new DateTime(2026, 2, 27, 14, 1, 52, 470, DateTimeKind.Local).AddTicks(3625) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 27, 14, 1, 52, 470, DateTimeKind.Local).AddTicks(2848), new DateTime(2026, 2, 27, 14, 1, 52, 470, DateTimeKind.Local).AddTicks(2859) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 27, 14, 1, 52, 470, DateTimeKind.Local).AddTicks(2860), new DateTime(2026, 2, 27, 14, 1, 52, 470, DateTimeKind.Local).AddTicks(2860) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 27, 14, 1, 52, 470, DateTimeKind.Local).AddTicks(2862), new DateTime(2026, 2, 27, 14, 1, 52, 470, DateTimeKind.Local).AddTicks(2862) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 27, 14, 1, 52, 470, DateTimeKind.Local).AddTicks(2863), new DateTime(2026, 2, 27, 14, 1, 52, 470, DateTimeKind.Local).AddTicks(2863) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 27, 14, 1, 52, 470, DateTimeKind.Local).AddTicks(2953), new DateTime(2026, 2, 27, 14, 1, 52, 470, DateTimeKind.Local).AddTicks(2953) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Device",
                table: "Attendances");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 9, 13, 11, 31, 955, DateTimeKind.Local).AddTicks(5462), new DateTime(2026, 2, 9, 13, 11, 31, 955, DateTimeKind.Local).AddTicks(5462) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 9, 13, 11, 31, 955, DateTimeKind.Local).AddTicks(5465), new DateTime(2026, 2, 9, 13, 11, 31, 955, DateTimeKind.Local).AddTicks(5465) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 9, 13, 11, 31, 955, DateTimeKind.Local).AddTicks(5466), new DateTime(2026, 2, 9, 13, 11, 31, 955, DateTimeKind.Local).AddTicks(5466) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 9, 13, 11, 31, 955, DateTimeKind.Local).AddTicks(4729), new DateTime(2026, 2, 9, 13, 11, 31, 955, DateTimeKind.Local).AddTicks(4741) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 9, 13, 11, 31, 955, DateTimeKind.Local).AddTicks(4743), new DateTime(2026, 2, 9, 13, 11, 31, 955, DateTimeKind.Local).AddTicks(4743) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 9, 13, 11, 31, 955, DateTimeKind.Local).AddTicks(4744), new DateTime(2026, 2, 9, 13, 11, 31, 955, DateTimeKind.Local).AddTicks(4744) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 9, 13, 11, 31, 955, DateTimeKind.Local).AddTicks(4745), new DateTime(2026, 2, 9, 13, 11, 31, 955, DateTimeKind.Local).AddTicks(4746) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 9, 13, 11, 31, 955, DateTimeKind.Local).AddTicks(4828), new DateTime(2026, 2, 9, 13, 11, 31, 955, DateTimeKind.Local).AddTicks(4828) });
        }
    }
}
