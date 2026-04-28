using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace arna.HRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeFiledNameAtToBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Users",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Users",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Timesheets",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Timesheets",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "LeaveRequests",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "LeaveRequests",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Employees",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Employees",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Departments",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Departments",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Attendances",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Attendances",
                newName: "CreatedBy");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 3, 12, 53, 56, 10, DateTimeKind.Local).AddTicks(529), new DateTime(2026, 1, 3, 12, 53, 56, 10, DateTimeKind.Local).AddTicks(539) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 3, 12, 53, 56, 10, DateTimeKind.Local).AddTicks(542), new DateTime(2026, 1, 3, 12, 53, 56, 10, DateTimeKind.Local).AddTicks(543) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 3, 12, 53, 56, 10, DateTimeKind.Local).AddTicks(544), new DateTime(2026, 1, 3, 12, 53, 56, 10, DateTimeKind.Local).AddTicks(545) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 3, 12, 53, 56, 10, DateTimeKind.Local).AddTicks(634), new DateTime(2026, 1, 3, 12, 53, 56, 10, DateTimeKind.Local).AddTicks(634) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 3, 12, 53, 56, 9, DateTimeKind.Local).AddTicks(4786), new DateTime(2026, 1, 3, 12, 53, 56, 9, DateTimeKind.Local).AddTicks(4800) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Users",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "Users",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Timesheets",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "Timesheets",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "LeaveRequests",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "LeaveRequests",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Employees",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "Employees",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Departments",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "Departments",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Attendances",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "Attendances",
                newName: "CreatedAt");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 7, 13, 38, 519, DateTimeKind.Utc).AddTicks(4704), new DateTime(2026, 1, 3, 7, 13, 38, 519, DateTimeKind.Utc).AddTicks(4706) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 7, 13, 38, 519, DateTimeKind.Utc).AddTicks(4709), new DateTime(2026, 1, 3, 7, 13, 38, 519, DateTimeKind.Utc).AddTicks(4710) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 7, 13, 38, 519, DateTimeKind.Utc).AddTicks(4711), new DateTime(2026, 1, 3, 7, 13, 38, 519, DateTimeKind.Utc).AddTicks(4711) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 7, 13, 38, 519, DateTimeKind.Utc).AddTicks(4712), new DateTime(2026, 1, 3, 7, 13, 38, 519, DateTimeKind.Utc).AddTicks(4713) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 3, 7, 13, 38, 518, DateTimeKind.Utc).AddTicks(7972), new DateTime(2026, 1, 3, 7, 13, 38, 518, DateTimeKind.Utc).AddTicks(7974) });
        }
    }
}
