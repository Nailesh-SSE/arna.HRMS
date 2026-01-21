using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace arna.HRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StoreLeaveStatusAsString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "LeaveRequests",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "LeaveRequests",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Pending");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 21, 13, 34, 49, 800, DateTimeKind.Local).AddTicks(1101), new DateTime(2026, 1, 21, 13, 34, 49, 800, DateTimeKind.Local).AddTicks(1101) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 21, 13, 34, 49, 800, DateTimeKind.Local).AddTicks(1103), new DateTime(2026, 1, 21, 13, 34, 49, 800, DateTimeKind.Local).AddTicks(1103) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 21, 13, 34, 49, 800, DateTimeKind.Local).AddTicks(1105), new DateTime(2026, 1, 21, 13, 34, 49, 800, DateTimeKind.Local).AddTicks(1105) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 21, 13, 34, 49, 800, DateTimeKind.Local).AddTicks(1106), new DateTime(2026, 1, 21, 13, 34, 49, 800, DateTimeKind.Local).AddTicks(1106) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 21, 13, 34, 49, 800, DateTimeKind.Local).AddTicks(1122), new DateTime(2026, 1, 21, 13, 34, 49, 800, DateTimeKind.Local).AddTicks(1123) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 21, 13, 34, 49, 800, DateTimeKind.Local).AddTicks(1124), new DateTime(2026, 1, 21, 13, 34, 49, 800, DateTimeKind.Local).AddTicks(1124) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 21, 13, 34, 49, 800, DateTimeKind.Local).AddTicks(1125), new DateTime(2026, 1, 21, 13, 34, 49, 800, DateTimeKind.Local).AddTicks(1125) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 21, 13, 34, 49, 800, DateTimeKind.Local).AddTicks(1126), new DateTime(2026, 1, 21, 13, 34, 49, 800, DateTimeKind.Local).AddTicks(1126) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 21, 13, 34, 49, 800, DateTimeKind.Local).AddTicks(1127), new DateTime(2026, 1, 21, 13, 34, 49, 800, DateTimeKind.Local).AddTicks(1127) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 21, 13, 34, 49, 800, DateTimeKind.Local).AddTicks(415), new DateTime(2026, 1, 21, 13, 34, 49, 800, DateTimeKind.Local).AddTicks(425) });
        }
    }
}
