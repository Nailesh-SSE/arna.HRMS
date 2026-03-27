using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace arna.HRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Create_Sp_EmployeeLeaveDetailsReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 27, 16, 16, 13, 870, DateTimeKind.Local).AddTicks(6791), new DateTime(2026, 3, 27, 16, 16, 13, 870, DateTimeKind.Local).AddTicks(6792) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 27, 16, 16, 13, 870, DateTimeKind.Local).AddTicks(6798), new DateTime(2026, 3, 27, 16, 16, 13, 870, DateTimeKind.Local).AddTicks(6798) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 27, 16, 16, 13, 870, DateTimeKind.Local).AddTicks(6801), new DateTime(2026, 3, 27, 16, 16, 13, 870, DateTimeKind.Local).AddTicks(6801) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 27, 16, 16, 13, 870, DateTimeKind.Local).AddTicks(5440), new DateTime(2026, 3, 27, 16, 16, 13, 870, DateTimeKind.Local).AddTicks(5455) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 27, 16, 16, 13, 870, DateTimeKind.Local).AddTicks(5459), new DateTime(2026, 3, 27, 16, 16, 13, 870, DateTimeKind.Local).AddTicks(5459) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 27, 16, 16, 13, 870, DateTimeKind.Local).AddTicks(5461), new DateTime(2026, 3, 27, 16, 16, 13, 870, DateTimeKind.Local).AddTicks(5461) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 27, 16, 16, 13, 870, DateTimeKind.Local).AddTicks(5463), new DateTime(2026, 3, 27, 16, 16, 13, 870, DateTimeKind.Local).AddTicks(5463) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 27, 16, 16, 13, 870, DateTimeKind.Local).AddTicks(5621), new DateTime(2026, 3, 27, 16, 16, 13, 870, DateTimeKind.Local).AddTicks(5622) });

            migrationBuilder.Sql(@" IF OBJECT_ID('dbo.Sp_EmployeeLeaveDetailsReport', 'P') IS NOT NULL DROP PROCEDURE dbo.Sp_EmployeeLeaveDetailsReport; ");

            migrationBuilder.Sql(@"
                 CREATE PROCEDURE dbo.Sp_EmployeeLeaveDetailsReport
            (
                @Year INT = NULL,
                @Month INT = NULL,
                @FromDate DATE = NULL,
                @ToDate DATE = NULL,
                @EmployeeId INT = NULL,
                @EmployeeNumber NVARCHAR(50) = NULL,
                @LeaveNameId INT = NULL
            )
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT 
                    e.Id AS EmployeeId,
                    e.EmployeeNumber,
                    CONCAT(e.FirstName, ' ', e.LastName) AS EmployeeName,

                    lt.Id AS LeaveTypeId,
                    lt.LeaveNameId,
                    lt.MaxPerYear,

                    lr.StartDate,
                    lr.EndDate,
                    ISNULL(lr.LeaveDays, 0) AS LeaveDays,
                    lr.Reason,
                    lr.StatusId,
                    lr.ApprovedBy,
                    lr.ApprovedDate

                FROM Employees e

                LEFT JOIN LeaveRequests lr 
                    ON e.Id = lr.EmployeeId 
                    AND lr.IsDeleted = 0
                    AND lr.IsActive = 1
                    AND lr.StatusId = 2
                    AND (@Year IS NULL OR YEAR(lr.StartDate) = @Year)
                    AND (@Month IS NULL OR MONTH(lr.StartDate) = @Month)
                    AND (
                        @FromDate IS NULL OR @ToDate IS NULL
                        OR (lr.StartDate <= @ToDate AND lr.EndDate >= @FromDate)
                    )

                LEFT JOIN LeaveTypes lt 
                    ON lt.Id = lr.LeaveTypeId
                    AND lt.IsDeleted = 0
                    AND lt.IsActive = 1

                WHERE 
                    e.IsDeleted = 0
                    AND e.IsActive = 1
                    AND (@EmployeeId IS NULL OR e.Id = @EmployeeId)
                    AND (@EmployeeNumber IS NULL OR e.EmployeeNumber = @EmployeeNumber)
                    AND (@LeaveNameId IS NULL OR lt.LeaveNameId = @LeaveNameId)

                ORDER BY 
                    e.Id,
                    ISNULL(lr.StartDate, '1900-01-01');
            END");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.Sql(@" IF OBJECT_ID('dbo.Sp_EmployeeLeaveDetailsReport', 'P') IS NOT NULL DROP PROCEDURE dbo.Sp_EmployeeLeaveDetailsReport; ");

        }
    }
}
