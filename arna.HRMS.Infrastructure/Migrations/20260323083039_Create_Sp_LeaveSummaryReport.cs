using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace arna.HRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Create_Sp_LeaveSummaryReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 23, 14, 0, 38, 545, DateTimeKind.Local).AddTicks(8119), new DateTime(2026, 3, 23, 14, 0, 38, 545, DateTimeKind.Local).AddTicks(8121) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 23, 14, 0, 38, 545, DateTimeKind.Local).AddTicks(8127), new DateTime(2026, 3, 23, 14, 0, 38, 545, DateTimeKind.Local).AddTicks(8128) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 23, 14, 0, 38, 545, DateTimeKind.Local).AddTicks(8130), new DateTime(2026, 3, 23, 14, 0, 38, 545, DateTimeKind.Local).AddTicks(8130) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 23, 14, 0, 38, 545, DateTimeKind.Local).AddTicks(6663), new DateTime(2026, 3, 23, 14, 0, 38, 545, DateTimeKind.Local).AddTicks(6680) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 23, 14, 0, 38, 545, DateTimeKind.Local).AddTicks(6684), new DateTime(2026, 3, 23, 14, 0, 38, 545, DateTimeKind.Local).AddTicks(6685) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 23, 14, 0, 38, 545, DateTimeKind.Local).AddTicks(6686), new DateTime(2026, 3, 23, 14, 0, 38, 545, DateTimeKind.Local).AddTicks(6687) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 23, 14, 0, 38, 545, DateTimeKind.Local).AddTicks(6688), new DateTime(2026, 3, 23, 14, 0, 38, 545, DateTimeKind.Local).AddTicks(6689) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 23, 14, 0, 38, 545, DateTimeKind.Local).AddTicks(6872), new DateTime(2026, 3, 23, 14, 0, 38, 545, DateTimeKind.Local).AddTicks(6873) });

            migrationBuilder.Sql(@"
                IF OBJECT_ID('dbo.Sp_LeaveSummaryReport', 'P') IS NOT NULL
                DROP PROCEDURE dbo.Sp_LeaveSummaryReport;
            ");

            migrationBuilder.Sql(@"
                CREATE PROCEDURE dbo.Sp_LeaveSummaryReport
                (
                    @Year INT = NULL,
                    @Month INT = NULL,
                    @FromDate DATE = NULL,
                    @ToDate DATE = NULL,
                    @DepartmentId INT = NULL
                )
                AS
                BEGIN
                    SET NOCOUNT ON;

                    SELECT 
                        e.Id AS EmployeeId,
                        e.EmployeeNumber,
                        CONCAT(e.FirstName, ' ', e.LastName) AS EmployeeName,
                        d.DepartmentName,

                        lt.LeaveNameId,
                        lt.MaxPerYear,

                        ISNULL(SUM(lr.LeaveDays), 0) AS UsedLeave

                    FROM Employees e
                    CROSS JOIN LeaveTypes lt
                    LEFT JOIN LeaveRequests lr 
                        ON lr.EmployeeId = e.Id
                        AND lr.LeaveTypeId = lt.Id
                        AND lr.IsDeleted = 0
                        AND lr.IsActive = 1
                        AND lr.StatusId = 2
                        AND (@Year IS NULL OR YEAR(lr.StartDate) = @Year)
                        AND (@Month IS NULL OR MONTH(lr.StartDate) = @Month)
                        AND (@FromDate IS NULL OR lr.StartDate >= @FromDate)
                        AND (@ToDate IS NULL OR lr.EndDate <= @ToDate)

                    LEFT JOIN Departments d 
                        ON d.Id = e.DepartmentId

                    WHERE 
                        e.IsDeleted = 0
                        AND e.IsActive = 1
                        AND lt.IsDeleted = 0
                        AND lt.IsActive = 1
                        AND (@DepartmentId IS NULL OR e.DepartmentId = @DepartmentId)

                    GROUP BY 
                        e.Id,
                        e.EmployeeNumber,
                        e.FirstName,
                        e.LastName,
                        d.DepartmentName,
                        lt.LeaveNameId,
                        lt.MaxPerYear
                END
                ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 17, 17, 36, 13, 696, DateTimeKind.Local).AddTicks(5746), new DateTime(2026, 3, 17, 17, 36, 13, 696, DateTimeKind.Local).AddTicks(5746) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 17, 17, 36, 13, 696, DateTimeKind.Local).AddTicks(5749), new DateTime(2026, 3, 17, 17, 36, 13, 696, DateTimeKind.Local).AddTicks(5750) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 17, 17, 36, 13, 696, DateTimeKind.Local).AddTicks(5751), new DateTime(2026, 3, 17, 17, 36, 13, 696, DateTimeKind.Local).AddTicks(5751) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 17, 17, 36, 13, 696, DateTimeKind.Local).AddTicks(5007), new DateTime(2026, 3, 17, 17, 36, 13, 696, DateTimeKind.Local).AddTicks(5017) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 17, 17, 36, 13, 696, DateTimeKind.Local).AddTicks(5018), new DateTime(2026, 3, 17, 17, 36, 13, 696, DateTimeKind.Local).AddTicks(5019) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 17, 17, 36, 13, 696, DateTimeKind.Local).AddTicks(5020), new DateTime(2026, 3, 17, 17, 36, 13, 696, DateTimeKind.Local).AddTicks(5020) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 17, 17, 36, 13, 696, DateTimeKind.Local).AddTicks(5021), new DateTime(2026, 3, 17, 17, 36, 13, 696, DateTimeKind.Local).AddTicks(5021) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 17, 17, 36, 13, 696, DateTimeKind.Local).AddTicks(5107), new DateTime(2026, 3, 17, 17, 36, 13, 696, DateTimeKind.Local).AddTicks(5108) });

            migrationBuilder.Sql(@"
                IF OBJECT_ID('dbo.Sp_LeaveSummaryReport', 'P') IS NOT NULL
                    DROP PROCEDURE dbo.Sp_LeaveSummaryReport;
            ");
        }
    }
}
