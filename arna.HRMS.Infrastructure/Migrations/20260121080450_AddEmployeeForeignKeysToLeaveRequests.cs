using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace arna.HRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeForeignKeysToLeaveRequests : Migration
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
                table: "LeaveRequests",
                newName: "UpdatedOn");

            migrationBuilder.RenameColumn(
                name: "LeaveType",
                table: "LeaveRequests",
                newName: "LeaveTypeId");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "LeaveRequests",
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
                name: "Date",
                table: "AttendanceRequest",
                newName: "ToDate");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "AttendanceRequest",
                newName: "FromDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "Roles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOn",
                table: "Roles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<int>(
                name: "TotalDays",
                table: "LeaveRequests",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "LeaveRequests",
                type: "int",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "ApprovalNotes",
                table: "LeaveRequests",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

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

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "BreakDuration",
                table: "AttendanceRequest",
                type: "time",
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "time");

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                table: "AttendanceRequest",
                type: "int",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedOn",
                table: "AttendanceRequest",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "AttendanceRequest",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "LeaveMasters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeaveName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MaxPerYear = table.Column<int>(type: "int", nullable: false),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveMasters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeLeaveBalances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    LeaveMasterId = table.Column<int>(type: "int", nullable: false),
                    TotalLeaves = table.Column<int>(type: "int", nullable: false),
                    UsedLeaves = table.Column<int>(type: "int", nullable: false),
                    RemainingLeaves = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeLeaveBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeLeaveBalances_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeLeaveBalances_LeaveMasters_LeaveMasterId",
                        column: x => x.LeaveMasterId,
                        principalTable: "LeaveMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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
                columns: new[] { "CreatedOn", "IsActive", "IsDeleted", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 21, 13, 34, 49, 800, DateTimeKind.Local).AddTicks(1122), true, false, new DateTime(2026, 1, 21, 13, 34, 49, 800, DateTimeKind.Local).AddTicks(1123) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "IsActive", "IsDeleted", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 21, 13, 34, 49, 800, DateTimeKind.Local).AddTicks(1124), true, false, new DateTime(2026, 1, 21, 13, 34, 49, 800, DateTimeKind.Local).AddTicks(1124) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "IsActive", "IsDeleted", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 21, 13, 34, 49, 800, DateTimeKind.Local).AddTicks(1125), true, false, new DateTime(2026, 1, 21, 13, 34, 49, 800, DateTimeKind.Local).AddTicks(1125) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedOn", "IsActive", "IsDeleted", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 21, 13, 34, 49, 800, DateTimeKind.Local).AddTicks(1126), true, false, new DateTime(2026, 1, 21, 13, 34, 49, 800, DateTimeKind.Local).AddTicks(1126) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedOn", "IsActive", "IsDeleted", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 21, 13, 34, 49, 800, DateTimeKind.Local).AddTicks(1127), true, false, new DateTime(2026, 1, 21, 13, 34, 49, 800, DateTimeKind.Local).AddTicks(1127) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 21, 13, 34, 49, 800, DateTimeKind.Local).AddTicks(415), new DateTime(2026, 1, 21, 13, 34, 49, 800, DateTimeKind.Local).AddTicks(425) });

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_LeaveTypeId",
                table: "LeaveRequests",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRequest_ApprovedBy",
                table: "AttendanceRequest",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLeaveBalances_EmployeeId",
                table: "EmployeeLeaveBalances",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLeaveBalances_EmployeeId_LeaveMasterId",
                table: "EmployeeLeaveBalances",
                columns: new[] { "EmployeeId", "LeaveMasterId" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLeaveBalances_LeaveMasterId",
                table: "EmployeeLeaveBalances",
                column: "LeaveMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveMasters_LeaveName",
                table: "LeaveMasters",
                column: "LeaveName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRequest_Employees_ApprovedBy",
                table: "AttendanceRequest",
                column: "ApprovedBy",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequests_LeaveMasters_LeaveTypeId",
                table: "LeaveRequests",
                column: "LeaveTypeId",
                principalTable: "LeaveMasters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRequest_Employees_ApprovedBy",
                table: "AttendanceRequest");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_LeaveMasters_LeaveTypeId",
                table: "LeaveRequests");

            migrationBuilder.DropTable(
                name: "EmployeeLeaveBalances");

            migrationBuilder.DropTable(
                name: "LeaveMasters");

            migrationBuilder.DropIndex(
                name: "IX_LeaveRequests_LeaveTypeId",
                table: "LeaveRequests");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceRequest_ApprovedBy",
                table: "AttendanceRequest");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "ApprovedOn",
                table: "AttendanceRequest");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "AttendanceRequest");

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
                table: "LeaveRequests",
                newName: "UpdatedBy");

            migrationBuilder.RenameColumn(
                name: "LeaveTypeId",
                table: "LeaveRequests",
                newName: "LeaveType");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "LeaveRequests",
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
                name: "ToDate",
                table: "AttendanceRequest",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "FromDate",
                table: "AttendanceRequest",
                newName: "CreatedBy");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalDays",
                table: "LeaveRequests",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "LeaveRequests",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "ApprovalNotes",
                table: "LeaveRequests",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

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

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "BreakDuration",
                table: "AttendanceRequest",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0),
                oldClrType: typeof(TimeSpan),
                oldType: "time",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ApprovedBy",
                table: "AttendanceRequest",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
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
