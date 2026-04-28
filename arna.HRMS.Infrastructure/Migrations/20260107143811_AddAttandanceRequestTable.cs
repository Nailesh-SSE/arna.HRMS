using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace arna.HRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAttandanceRequestTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AttendanceRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReasonType = table.Column<int>(type: "int", nullable: false),
                    Location = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    ClockIn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClockOut = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BreakDuration = table.Column<TimeSpan>(type: "time", nullable: false),
                    TotalHours = table.Column<TimeSpan>(type: "time", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ApprovedBy = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceRequest_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 7, 20, 8, 9, 154, DateTimeKind.Local).AddTicks(2046), new DateTime(2026, 1, 7, 20, 8, 9, 154, DateTimeKind.Local).AddTicks(2048) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 7, 20, 8, 9, 154, DateTimeKind.Local).AddTicks(2051), new DateTime(2026, 1, 7, 20, 8, 9, 154, DateTimeKind.Local).AddTicks(2051) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 7, 20, 8, 9, 154, DateTimeKind.Local).AddTicks(2055), new DateTime(2026, 1, 7, 20, 8, 9, 154, DateTimeKind.Local).AddTicks(2056) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 7, 20, 8, 9, 154, DateTimeKind.Local).AddTicks(2057), new DateTime(2026, 1, 7, 20, 8, 9, 154, DateTimeKind.Local).AddTicks(2057) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 7, 20, 8, 9, 154, DateTimeKind.Local).AddTicks(1057), new DateTime(2026, 1, 7, 20, 8, 9, 154, DateTimeKind.Local).AddTicks(1067) });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRequest_EmployeeId",
                table: "AttendanceRequest",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceRequest");

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
    }
}
