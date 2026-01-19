using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace arna.HRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeFieldNameByToOn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Users",
                newName: "UpdatedOn");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "Users",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Timesheets",
                newName: "UpdatedOn");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "Timesheets",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Roles",
                newName: "UpdatedOn");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "Roles",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "LeaveRequests",
                newName: "UpdatedOn");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "LeaveRequests",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "LeaveMasters",
                newName: "UpdatedOn");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "LeaveMasters",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "FestivalHoliday",
                newName: "UpdatedOn");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "FestivalHoliday",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Employees",
                newName: "UpdatedOn");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "Employees",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "EmployeeLeaveBalances",
                newName: "UpdatedOn");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "EmployeeLeaveBalances",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Departments",
                newName: "UpdatedOn");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "Departments",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "Attendances",
                newName: "UpdatedOn");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "Attendances",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "UpdatedBy",
                table: "AttendanceRequest",
                newName: "UpdatedOn");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "AttendanceRequest",
                newName: "CreatedOn");

            migrationBuilder.RenameColumn(
                name: "ApprovedBy",
                table: "AttendanceRequest",
                newName: "ApprovedOn");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 19, 19, 31, 3, 969, DateTimeKind.Local).AddTicks(9023), new DateTime(2026, 1, 19, 19, 31, 3, 969, DateTimeKind.Local).AddTicks(9024) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 19, 19, 31, 3, 969, DateTimeKind.Local).AddTicks(9026), new DateTime(2026, 1, 19, 19, 31, 3, 969, DateTimeKind.Local).AddTicks(9026) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 19, 19, 31, 3, 969, DateTimeKind.Local).AddTicks(9028), new DateTime(2026, 1, 19, 19, 31, 3, 969, DateTimeKind.Local).AddTicks(9028) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 19, 19, 31, 3, 969, DateTimeKind.Local).AddTicks(9029), new DateTime(2026, 1, 19, 19, 31, 3, 969, DateTimeKind.Local).AddTicks(9029) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 19, 19, 31, 3, 969, DateTimeKind.Local).AddTicks(9563), new DateTime(2026, 1, 19, 19, 31, 3, 969, DateTimeKind.Local).AddTicks(9567) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 19, 19, 31, 3, 969, DateTimeKind.Local).AddTicks(9576), new DateTime(2026, 1, 19, 19, 31, 3, 969, DateTimeKind.Local).AddTicks(9576) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 19, 19, 31, 3, 969, DateTimeKind.Local).AddTicks(9577), new DateTime(2026, 1, 19, 19, 31, 3, 969, DateTimeKind.Local).AddTicks(9578) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 19, 19, 31, 3, 969, DateTimeKind.Local).AddTicks(9579), new DateTime(2026, 1, 19, 19, 31, 3, 969, DateTimeKind.Local).AddTicks(9579) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 19, 19, 31, 3, 969, DateTimeKind.Local).AddTicks(9580), new DateTime(2026, 1, 19, 19, 31, 3, 969, DateTimeKind.Local).AddTicks(9580) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 19, 19, 31, 3, 969, DateTimeKind.Local).AddTicks(8383), new DateTime(2026, 1, 19, 19, 31, 3, 969, DateTimeKind.Local).AddTicks(8393) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedOn",
                table: "Users",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "Users",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedOn",
                table: "Timesheets",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "Timesheets",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedOn",
                table: "Roles",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "Roles",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedOn",
                table: "LeaveRequests",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "LeaveRequests",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedOn",
                table: "LeaveMasters",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "LeaveMasters",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedOn",
                table: "FestivalHoliday",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "FestivalHoliday",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedOn",
                table: "Employees",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "Employees",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedOn",
                table: "EmployeeLeaveBalances",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "EmployeeLeaveBalances",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedOn",
                table: "Departments",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "Departments",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedOn",
                table: "Attendances",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "Attendances",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "UpdatedOn",
                table: "AttendanceRequest",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "AttendanceRequest",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "ApprovedOn",
                table: "AttendanceRequest",
                newName: "ApprovedBy");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 19, 17, 14, 12, 296, DateTimeKind.Local).AddTicks(7844), new DateTime(2026, 1, 19, 17, 14, 12, 296, DateTimeKind.Local).AddTicks(7845) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 19, 17, 14, 12, 296, DateTimeKind.Local).AddTicks(7847), new DateTime(2026, 1, 19, 17, 14, 12, 296, DateTimeKind.Local).AddTicks(7848) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 19, 17, 14, 12, 296, DateTimeKind.Local).AddTicks(7849), new DateTime(2026, 1, 19, 17, 14, 12, 296, DateTimeKind.Local).AddTicks(7849) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 19, 17, 14, 12, 296, DateTimeKind.Local).AddTicks(7850), new DateTime(2026, 1, 19, 17, 14, 12, 296, DateTimeKind.Local).AddTicks(7851) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 19, 17, 14, 12, 296, DateTimeKind.Local).AddTicks(7866), new DateTime(2026, 1, 19, 17, 14, 12, 296, DateTimeKind.Local).AddTicks(7866) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 19, 17, 14, 12, 296, DateTimeKind.Local).AddTicks(7868), new DateTime(2026, 1, 19, 17, 14, 12, 296, DateTimeKind.Local).AddTicks(7869) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 19, 17, 14, 12, 296, DateTimeKind.Local).AddTicks(7870), new DateTime(2026, 1, 19, 17, 14, 12, 296, DateTimeKind.Local).AddTicks(7870) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 19, 17, 14, 12, 296, DateTimeKind.Local).AddTicks(7870), new DateTime(2026, 1, 19, 17, 14, 12, 296, DateTimeKind.Local).AddTicks(7871) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 19, 17, 14, 12, 296, DateTimeKind.Local).AddTicks(7871), new DateTime(2026, 1, 19, 17, 14, 12, 296, DateTimeKind.Local).AddTicks(7872) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 19, 17, 14, 12, 296, DateTimeKind.Local).AddTicks(7173), new DateTime(2026, 1, 19, 17, 14, 12, 296, DateTimeKind.Local).AddTicks(7184) });
        }
    }
}
