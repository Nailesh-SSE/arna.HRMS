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
                values: new object[] { new DateTime(2026, 3, 25, 13, 7, 57, 220, DateTimeKind.Local).AddTicks(8253), new DateTime(2026, 3, 25, 13, 7, 57, 220, DateTimeKind.Local).AddTicks(8254) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 25, 13, 7, 57, 220, DateTimeKind.Local).AddTicks(8261), new DateTime(2026, 3, 25, 13, 7, 57, 220, DateTimeKind.Local).AddTicks(8261) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 25, 13, 7, 57, 220, DateTimeKind.Local).AddTicks(8264), new DateTime(2026, 3, 25, 13, 7, 57, 220, DateTimeKind.Local).AddTicks(8265) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 25, 13, 7, 57, 220, DateTimeKind.Local).AddTicks(6571), new DateTime(2026, 3, 25, 13, 7, 57, 220, DateTimeKind.Local).AddTicks(6597) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 25, 13, 7, 57, 220, DateTimeKind.Local).AddTicks(6603), new DateTime(2026, 3, 25, 13, 7, 57, 220, DateTimeKind.Local).AddTicks(6603) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 25, 13, 7, 57, 220, DateTimeKind.Local).AddTicks(6605), new DateTime(2026, 3, 25, 13, 7, 57, 220, DateTimeKind.Local).AddTicks(6606) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 25, 13, 7, 57, 220, DateTimeKind.Local).AddTicks(6607), new DateTime(2026, 3, 25, 13, 7, 57, 220, DateTimeKind.Local).AddTicks(6608) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 25, 13, 7, 57, 220, DateTimeKind.Local).AddTicks(6840), new DateTime(2026, 3, 25, 13, 7, 57, 220, DateTimeKind.Local).AddTicks(6841) });

            migrationBuilder.Sql(@" IF OBJECT_ID('dbo.Sp_EmployeeLeaveDetailsReport', 'P') IS NOT NULL DROP PROCEDURE dbo.Sp_EmployeeLeaveDetailsReport; ");

            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[Sp_EmployeeLeaveDetailsReport]
                (
                    @Year INT = NULL,
                    @Month INT = NULL,
                    @FromDate DATE = NULL,
                    @ToDate DATE = NULL,
                    @EmployeeId INT = NULL,
                    @EmployeeNumber NVARCHAR(50) = NULL
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

                    ORDER BY 
                        e.Id,
                        ISNULL(lr.StartDate, '1900-01-01');
                END;
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

            migrationBuilder.Sql(@" IF OBJECT_ID('dbo.Sp_EmployeeLeaveDetailsReport', 'P') IS NOT NULL DROP PROCEDURE dbo.Sp_EmployeeLeaveDetailsReport; ");
        }
    }
}
