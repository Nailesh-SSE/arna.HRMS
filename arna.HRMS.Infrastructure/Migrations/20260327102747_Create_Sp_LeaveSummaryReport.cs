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
                values: new object[] { new DateTime(2026, 3, 27, 15, 57, 46, 945, DateTimeKind.Local).AddTicks(1007), new DateTime(2026, 3, 27, 15, 57, 46, 945, DateTimeKind.Local).AddTicks(1008) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 27, 15, 57, 46, 945, DateTimeKind.Local).AddTicks(1014), new DateTime(2026, 3, 27, 15, 57, 46, 945, DateTimeKind.Local).AddTicks(1014) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 27, 15, 57, 46, 945, DateTimeKind.Local).AddTicks(1016), new DateTime(2026, 3, 27, 15, 57, 46, 945, DateTimeKind.Local).AddTicks(1017) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 27, 15, 57, 46, 944, DateTimeKind.Local).AddTicks(9471), new DateTime(2026, 3, 27, 15, 57, 46, 944, DateTimeKind.Local).AddTicks(9488) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 27, 15, 57, 46, 944, DateTimeKind.Local).AddTicks(9491), new DateTime(2026, 3, 27, 15, 57, 46, 944, DateTimeKind.Local).AddTicks(9492) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 27, 15, 57, 46, 944, DateTimeKind.Local).AddTicks(9493), new DateTime(2026, 3, 27, 15, 57, 46, 944, DateTimeKind.Local).AddTicks(9494) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 27, 15, 57, 46, 944, DateTimeKind.Local).AddTicks(9495), new DateTime(2026, 3, 27, 15, 57, 46, 944, DateTimeKind.Local).AddTicks(9495) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 27, 15, 57, 46, 944, DateTimeKind.Local).AddTicks(9661), new DateTime(2026, 3, 27, 15, 57, 46, 944, DateTimeKind.Local).AddTicks(9661) });

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
                @DepartmentId INT = NULL,
                @LeaveNameId INT = NULL   -- ✅ FIXED
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
                    AND (@LeaveNameId IS NULL OR lt.LeaveNameId = @LeaveNameId)

                GROUP BY 
                    e.Id,
                    e.EmployeeNumber,
                    e.FirstName,
                    e.LastName,
                    d.DepartmentName,
                    lt.LeaveNameId,
                    lt.MaxPerYear

                ORDER BY 
                    e.Id,
                    lt.LeaveNameId
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
