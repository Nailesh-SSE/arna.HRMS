using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace arna.HRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixAttendanceDevice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Device",
                table: "Attendances");

            migrationBuilder.AddColumn<int>(
                name: "DeviceId",
                table: "Attendances",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 13, 15, 58, 30, 358, DateTimeKind.Local).AddTicks(4789), new DateTime(2026, 3, 13, 15, 58, 30, 358, DateTimeKind.Local).AddTicks(4789) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 13, 15, 58, 30, 358, DateTimeKind.Local).AddTicks(4792), new DateTime(2026, 3, 13, 15, 58, 30, 358, DateTimeKind.Local).AddTicks(4792) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 13, 15, 58, 30, 358, DateTimeKind.Local).AddTicks(4793), new DateTime(2026, 3, 13, 15, 58, 30, 358, DateTimeKind.Local).AddTicks(4794) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 13, 15, 58, 30, 358, DateTimeKind.Local).AddTicks(3945), new DateTime(2026, 3, 13, 15, 58, 30, 358, DateTimeKind.Local).AddTicks(3956) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 13, 15, 58, 30, 358, DateTimeKind.Local).AddTicks(3957), new DateTime(2026, 3, 13, 15, 58, 30, 358, DateTimeKind.Local).AddTicks(3958) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 13, 15, 58, 30, 358, DateTimeKind.Local).AddTicks(3959), new DateTime(2026, 3, 13, 15, 58, 30, 358, DateTimeKind.Local).AddTicks(3959) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 13, 15, 58, 30, 358, DateTimeKind.Local).AddTicks(3960), new DateTime(2026, 3, 13, 15, 58, 30, 358, DateTimeKind.Local).AddTicks(3960) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 13, 15, 58, 30, 358, DateTimeKind.Local).AddTicks(4060), new DateTime(2026, 3, 13, 15, 58, 30, 358, DateTimeKind.Local).AddTicks(4061) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "Attendances");

            migrationBuilder.AddColumn<string>(
                name: "Device",
                table: "Attendances",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 10, 15, 11, 15, 22, DateTimeKind.Local).AddTicks(8978), new DateTime(2026, 3, 10, 15, 11, 15, 22, DateTimeKind.Local).AddTicks(8989) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 10, 15, 11, 15, 22, DateTimeKind.Local).AddTicks(8994), new DateTime(2026, 3, 10, 15, 11, 15, 22, DateTimeKind.Local).AddTicks(8994) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 10, 15, 11, 15, 22, DateTimeKind.Local).AddTicks(8997), new DateTime(2026, 3, 10, 15, 11, 15, 22, DateTimeKind.Local).AddTicks(8997) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 10, 15, 11, 15, 22, DateTimeKind.Local).AddTicks(3655), new DateTime(2026, 3, 10, 15, 11, 15, 22, DateTimeKind.Local).AddTicks(3670) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 10, 15, 11, 15, 22, DateTimeKind.Local).AddTicks(3675), new DateTime(2026, 3, 10, 15, 11, 15, 22, DateTimeKind.Local).AddTicks(3675) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 10, 15, 11, 15, 22, DateTimeKind.Local).AddTicks(3677), new DateTime(2026, 3, 10, 15, 11, 15, 22, DateTimeKind.Local).AddTicks(3678) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 10, 15, 11, 15, 22, DateTimeKind.Local).AddTicks(3679), new DateTime(2026, 3, 10, 15, 11, 15, 22, DateTimeKind.Local).AddTicks(3680) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 10, 15, 11, 15, 22, DateTimeKind.Local).AddTicks(3874), new DateTime(2026, 3, 10, 15, 11, 15, 22, DateTimeKind.Local).AddTicks(3875) });
        }
    }
}
