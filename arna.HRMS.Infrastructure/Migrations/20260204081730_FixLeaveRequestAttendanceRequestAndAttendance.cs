using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace arna.HRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixLeaveRequestAttendanceRequestAndAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "AttendanceRequest");

            migrationBuilder.DropColumn(
                name: "ReasonType",
                table: "AttendanceRequest");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AttendanceRequest");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Attendances",
                newName: "StatusId");

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "LeaveRequests",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                table: "AttendanceRequest",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReasonTypeId",
                table: "AttendanceRequest",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "AttendanceRequest",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 4, 13, 47, 26, 974, DateTimeKind.Local).AddTicks(5326), new DateTime(2026, 2, 4, 13, 47, 26, 974, DateTimeKind.Local).AddTicks(5326) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 4, 13, 47, 26, 974, DateTimeKind.Local).AddTicks(5329), new DateTime(2026, 2, 4, 13, 47, 26, 974, DateTimeKind.Local).AddTicks(5329) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 4, 13, 47, 26, 974, DateTimeKind.Local).AddTicks(5330), new DateTime(2026, 2, 4, 13, 47, 26, 974, DateTimeKind.Local).AddTicks(5330) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 4, 13, 47, 26, 974, DateTimeKind.Local).AddTicks(4655), new DateTime(2026, 2, 4, 13, 47, 26, 974, DateTimeKind.Local).AddTicks(4665) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 4, 13, 47, 26, 974, DateTimeKind.Local).AddTicks(4668), new DateTime(2026, 2, 4, 13, 47, 26, 974, DateTimeKind.Local).AddTicks(4668) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 4, 13, 47, 26, 974, DateTimeKind.Local).AddTicks(4669), new DateTime(2026, 2, 4, 13, 47, 26, 974, DateTimeKind.Local).AddTicks(4669) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 4, 13, 47, 26, 974, DateTimeKind.Local).AddTicks(4670), new DateTime(2026, 2, 4, 13, 47, 26, 974, DateTimeKind.Local).AddTicks(4670) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 4, 13, 47, 26, 974, DateTimeKind.Local).AddTicks(4731), new DateTime(2026, 2, 4, 13, 47, 26, 974, DateTimeKind.Local).AddTicks(4732) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "AttendanceRequest");

            migrationBuilder.DropColumn(
                name: "ReasonTypeId",
                table: "AttendanceRequest");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "AttendanceRequest");

            migrationBuilder.RenameColumn(
                name: "StatusId",
                table: "Attendances",
                newName: "Status");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "LeaveRequests",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "AttendanceRequest",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReasonType",
                table: "AttendanceRequest",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

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
                values: new object[] { new DateTime(2026, 1, 29, 18, 55, 51, 759, DateTimeKind.Local).AddTicks(5600), new DateTime(2026, 1, 29, 18, 55, 51, 759, DateTimeKind.Local).AddTicks(5606) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 29, 18, 55, 51, 759, DateTimeKind.Local).AddTicks(5609), new DateTime(2026, 1, 29, 18, 55, 51, 759, DateTimeKind.Local).AddTicks(5610) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 29, 18, 55, 51, 759, DateTimeKind.Local).AddTicks(5611), new DateTime(2026, 1, 29, 18, 55, 51, 759, DateTimeKind.Local).AddTicks(5612) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 29, 18, 55, 51, 759, DateTimeKind.Local).AddTicks(2090), new DateTime(2026, 1, 29, 18, 55, 51, 759, DateTimeKind.Local).AddTicks(2100) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 29, 18, 55, 51, 759, DateTimeKind.Local).AddTicks(2102), new DateTime(2026, 1, 29, 18, 55, 51, 759, DateTimeKind.Local).AddTicks(2103) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 29, 18, 55, 51, 759, DateTimeKind.Local).AddTicks(2104), new DateTime(2026, 1, 29, 18, 55, 51, 759, DateTimeKind.Local).AddTicks(2104) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 29, 18, 55, 51, 759, DateTimeKind.Local).AddTicks(2105), new DateTime(2026, 1, 29, 18, 55, 51, 759, DateTimeKind.Local).AddTicks(2105) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 29, 18, 55, 51, 759, DateTimeKind.Local).AddTicks(2177), new DateTime(2026, 1, 29, 18, 55, 51, 759, DateTimeKind.Local).AddTicks(2178) });
        }
    }
}
