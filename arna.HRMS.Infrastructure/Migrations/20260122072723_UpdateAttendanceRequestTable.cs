using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace arna.HRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAttendanceRequestTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "AttendanceRequest");

            migrationBuilder.AlterColumn<string>(
                name: "ReasonType",
                table: "AttendanceRequest",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "AttendanceRequest",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "AttendanceRequest",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(7606), new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(7607) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(7609), new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(7609) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(7610), new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(7611) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(7612), new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(7612) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(7627), new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(7627) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(7628), new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(7629) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(7630), new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(7630) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(7631), new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(7631) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(7632), new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(7632) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(6959), new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(6968) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "AttendanceRequest");

            migrationBuilder.AlterColumn<string>(
                name: "ReasonType",
                table: "AttendanceRequest",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(60)",
                oldMaxLength: 60);

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "AttendanceRequest",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "AttendanceRequest",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 21, 16, 47, 44, 348, DateTimeKind.Local).AddTicks(282), new DateTime(2026, 1, 21, 16, 47, 44, 348, DateTimeKind.Local).AddTicks(303) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 21, 16, 47, 44, 348, DateTimeKind.Local).AddTicks(311), new DateTime(2026, 1, 21, 16, 47, 44, 348, DateTimeKind.Local).AddTicks(313) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 21, 16, 47, 44, 348, DateTimeKind.Local).AddTicks(317), new DateTime(2026, 1, 21, 16, 47, 44, 348, DateTimeKind.Local).AddTicks(318) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 21, 16, 47, 44, 348, DateTimeKind.Local).AddTicks(322), new DateTime(2026, 1, 21, 16, 47, 44, 348, DateTimeKind.Local).AddTicks(323) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 21, 16, 47, 44, 348, DateTimeKind.Local).AddTicks(406), new DateTime(2026, 1, 21, 16, 47, 44, 348, DateTimeKind.Local).AddTicks(408) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 21, 16, 47, 44, 348, DateTimeKind.Local).AddTicks(412), new DateTime(2026, 1, 21, 16, 47, 44, 348, DateTimeKind.Local).AddTicks(413) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 21, 16, 47, 44, 348, DateTimeKind.Local).AddTicks(416), new DateTime(2026, 1, 21, 16, 47, 44, 348, DateTimeKind.Local).AddTicks(417) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 21, 16, 47, 44, 348, DateTimeKind.Local).AddTicks(420), new DateTime(2026, 1, 21, 16, 47, 44, 348, DateTimeKind.Local).AddTicks(421) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 21, 16, 47, 44, 348, DateTimeKind.Local).AddTicks(424), new DateTime(2026, 1, 21, 16, 47, 44, 348, DateTimeKind.Local).AddTicks(425) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 21, 16, 47, 44, 345, DateTimeKind.Local).AddTicks(7848), new DateTime(2026, 1, 21, 16, 47, 44, 345, DateTimeKind.Local).AddTicks(7873) });
        }
    }
}
