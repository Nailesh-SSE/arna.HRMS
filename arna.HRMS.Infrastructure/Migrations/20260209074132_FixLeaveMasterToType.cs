using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace arna.HRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixLeaveMasterToType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_LeaveMasters_LeaveTypeId",
                table: "LeaveRequests");

            migrationBuilder.DropTable(
                name: "EmployeeLeaveBalances");

            migrationBuilder.DropTable(
                name: "LeaveMasters");

            migrationBuilder.RenameColumn(
                name: "TotalDays",
                table: "LeaveRequests",
                newName: "LeaveDays");

            migrationBuilder.AddColumn<string>(
                name: "DayOfWeek",
                table: "FestivalHoliday",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "LeaveTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeaveNameId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_LeaveTypes", x => x.Id);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_LeaveTypes_LeaveNameId",
                table: "LeaveTypes",
                column: "LeaveNameId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LeaveRequests_LeaveTypes_LeaveTypeId",
                table: "LeaveRequests",
                column: "LeaveTypeId",
                principalTable: "LeaveTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaveRequests_LeaveTypes_LeaveTypeId",
                table: "LeaveRequests");

            migrationBuilder.DropTable(
                name: "LeaveTypes");

            migrationBuilder.DropColumn(
                name: "DayOfWeek",
                table: "FestivalHoliday");

            migrationBuilder.RenameColumn(
                name: "LeaveDays",
                table: "LeaveRequests",
                newName: "TotalDays");

            migrationBuilder.CreateTable(
                name: "LeaveMasters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false),
                    LeaveName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MaxPerYear = table.Column<int>(type: "int", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RemainingLeaves = table.Column<int>(type: "int", nullable: false),
                    TotalLeaves = table.Column<int>(type: "int", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsedLeaves = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false)
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
                name: "FK_LeaveRequests_LeaveMasters_LeaveTypeId",
                table: "LeaveRequests",
                column: "LeaveTypeId",
                principalTable: "LeaveMasters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
