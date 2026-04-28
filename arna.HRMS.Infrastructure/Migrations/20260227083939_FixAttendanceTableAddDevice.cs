using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace arna.HRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixAttendanceTableAddDevice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Device",
                table: "Attendances",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50);

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 27, 14, 9, 36, 296, DateTimeKind.Local).AddTicks(4308), new DateTime(2026, 2, 27, 14, 9, 36, 296, DateTimeKind.Local).AddTicks(4308) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 27, 14, 9, 36, 296, DateTimeKind.Local).AddTicks(4313), new DateTime(2026, 2, 27, 14, 9, 36, 296, DateTimeKind.Local).AddTicks(4313) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 27, 14, 9, 36, 296, DateTimeKind.Local).AddTicks(4314), new DateTime(2026, 2, 27, 14, 9, 36, 296, DateTimeKind.Local).AddTicks(4314) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 27, 14, 9, 36, 296, DateTimeKind.Local).AddTicks(3618), new DateTime(2026, 2, 27, 14, 9, 36, 296, DateTimeKind.Local).AddTicks(3629) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 27, 14, 9, 36, 296, DateTimeKind.Local).AddTicks(3630), new DateTime(2026, 2, 27, 14, 9, 36, 296, DateTimeKind.Local).AddTicks(3630) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 27, 14, 9, 36, 296, DateTimeKind.Local).AddTicks(3631), new DateTime(2026, 2, 27, 14, 9, 36, 296, DateTimeKind.Local).AddTicks(3632) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 27, 14, 9, 36, 296, DateTimeKind.Local).AddTicks(3633), new DateTime(2026, 2, 27, 14, 9, 36, 296, DateTimeKind.Local).AddTicks(3633) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 27, 14, 9, 36, 296, DateTimeKind.Local).AddTicks(3715), new DateTime(2026, 2, 27, 14, 9, 36, 296, DateTimeKind.Local).AddTicks(3716) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Device",
                table: "Attendances",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

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
    }
}
