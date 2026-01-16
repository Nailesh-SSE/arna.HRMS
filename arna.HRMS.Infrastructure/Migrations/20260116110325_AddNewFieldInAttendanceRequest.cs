using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace arna.HRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewFieldInAttendanceRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Date",
                table: "AttendanceRequest",
                newName: "ToDate");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "BreakDuration",
                table: "AttendanceRequest",
                type: "time",
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "time");

            migrationBuilder.AddColumn<DateTime>(
                name: "FromDate",
                table: "AttendanceRequest",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FromDate",
                table: "AttendanceRequest");

            migrationBuilder.RenameColumn(
                name: "ToDate",
                table: "AttendanceRequest",
                newName: "Date");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "BreakDuration",
                table: "AttendanceRequest",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0),
                oldClrType: typeof(TimeSpan),
                oldType: "time",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 9, 19, 49, 6, 682, DateTimeKind.Local).AddTicks(5113), new DateTime(2026, 1, 9, 19, 49, 6, 682, DateTimeKind.Local).AddTicks(5113) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 9, 19, 49, 6, 682, DateTimeKind.Local).AddTicks(5115), new DateTime(2026, 1, 9, 19, 49, 6, 682, DateTimeKind.Local).AddTicks(5116) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 9, 19, 49, 6, 682, DateTimeKind.Local).AddTicks(5117), new DateTime(2026, 1, 9, 19, 49, 6, 682, DateTimeKind.Local).AddTicks(5117) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 9, 19, 49, 6, 682, DateTimeKind.Local).AddTicks(5118), new DateTime(2026, 1, 9, 19, 49, 6, 682, DateTimeKind.Local).AddTicks(5118) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 9, 19, 49, 6, 682, DateTimeKind.Local).AddTicks(4467), new DateTime(2026, 1, 9, 19, 49, 6, 682, DateTimeKind.Local).AddTicks(4481) });
        }
    }
}
