using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace arna.HRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Create_Sp_EmployeeAttendanceReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF OBJECT_ID('dbo.Sp_EmployeesAttendanceReport', 'P') IS NOT NULL
                DROP PROCEDURE dbo.Sp_EmployeesAttendanceReport;
            ");

            migrationBuilder.Sql(@"
                CREATE PROCEDURE dbo.Sp_EmployeesAttendanceReport
                (
                    @Year INT = NULL,
                    @Month INT = NULL,
                    @EmployeeId INT = NULL,
                    @FromDate DATE = NULL,
                    @ToDate DATE = NULL
                )
                AS
                BEGIN
                    SET NOCOUNT ON;
                    SELECT
                        EmployeeId,
		                EmployeeNumber,
                        EmployeeName,

                        COUNT(*) AS TotalWorkDays,

                        SUM(CASE WHEN AttendanceStatus IN (1,3,4) THEN 1 ELSE 0 END) AS TotalPresent,

                        SUM(CASE WHEN AttendanceStatus IN (2,5) THEN 1 ELSE 0 END) AS TotalAbsent,    

                        -- TOTAL HOURS
                        CONCAT(
                            SUM(TotalSeconds) / 3600, ':',
                            RIGHT('00' + CAST((SUM(TotalSeconds) % 3600) / 60 AS VARCHAR),2), ':',
                            RIGHT('00' + CAST(SUM(TotalSeconds) % 60 AS VARCHAR),2)
                        ) AS TotalHours,

                        -- AVERAGE WORK HOURS
                        CONCAT(
                            AVG(WorkingSeconds) / 3600, ':',
                            RIGHT('00' + CAST((AVG(WorkingSeconds) % 3600) / 60 AS VARCHAR),2), ':',
                            RIGHT('00' + CAST(AVG(WorkingSeconds) % 60 AS VARCHAR),2)
                        ) AS AvgWorkHours,

                        -- AVERAGE BREAK
                        CONCAT(
                            AVG(BreakSeconds) / 3600, ':',
                            RIGHT('00' + CAST((AVG(BreakSeconds) % 3600) / 60 AS VARCHAR),2), ':',
                            RIGHT('00' + CAST(AVG(BreakSeconds) % 60 AS VARCHAR),2)
                        ) AS AvgBreak

                    FROM
                    (
                        SELECT
                            a.EmployeeId,
			                e.EmployeeNumber,
                            CONCAT(e.FirstName,' ',e.LastName) AS EmployeeName,
                            CAST(a.Date AS DATE) AS AttendanceDate,

                            -- WORKING SECONDS
                            SUM(
                                CASE
                                    WHEN a.TotalHours IS NOT NULL
                                    THEN DATEDIFF(SECOND,0,a.TotalHours)
                                    ELSE 0
                                END
                            ) AS WorkingSeconds,

                            -- TOTAL HOURS (ClockIn -> ClockOut)
                            CAST(
                                DATEADD(
                                    SECOND,
                                    DATEDIFF(SECOND, MIN(a.ClockIn), MAX(a.ClockOut)),
                                    0
                                ) AS TIME
                            ) AS TotalHours,

                            DATEDIFF(SECOND, MIN(a.ClockIn), MAX(a.ClockOut)) AS TotalSeconds,

                            -- BREAK SECONDS
                            CASE
                                WHEN MIN(a.ClockIn) IS NULL OR MAX(a.ClockOut) IS NULL
                                THEN 0
                                ELSE
                                    DATEDIFF(SECOND, MIN(CAST(a.ClockIn AS TIME)), MAX(CAST(a.ClockOut AS TIME)))
                                    -
                                    SUM(
                                        CASE
                                            WHEN a.TotalHours IS NOT NULL
                                            THEN DATEDIFF(SECOND,0,a.TotalHours)
                                            ELSE 0
                                        END
                                    )
                            END AS BreakSeconds,

                            MAX(a.StatusId) AS AttendanceStatus

                        FROM Attendances a
                        INNER JOIN Employees e ON e.Id = a.EmployeeId

                        WHERE
                            a.IsActive = 1
                            AND a.IsDeleted = 0
                            AND (@EmployeeId IS NULL OR a.EmployeeId = @EmployeeId)
                            AND (@Year IS NULL OR YEAR(a.Date) = @Year)
                            AND (@Month IS NULL OR MONTH(a.Date) = @Month)
                            AND (@FromDate IS NULL OR CAST(a.Date AS DATE) >= @FromDate)
                            AND (@ToDate IS NULL OR CAST(a.Date AS DATE) <= @ToDate)

                        GROUP BY
                            a.EmployeeId,
			                e.EmployeeNumber,
                            e.FirstName,
                            e.LastName,
                            CAST(a.Date AS DATE)

                    ) AS Daily

                    GROUP BY
                        EmployeeId,
		                EmployeeNumber,
                        EmployeeName

                    ORDER BY
                        EmployeeId; 
                END");



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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 17, 13, 59, 41, 976, DateTimeKind.Local).AddTicks(5071), new DateTime(2026, 3, 17, 13, 59, 41, 976, DateTimeKind.Local).AddTicks(5087) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 17, 13, 59, 41, 976, DateTimeKind.Local).AddTicks(5097), new DateTime(2026, 3, 17, 13, 59, 41, 976, DateTimeKind.Local).AddTicks(5098) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 17, 13, 59, 41, 976, DateTimeKind.Local).AddTicks(5103), new DateTime(2026, 3, 17, 13, 59, 41, 976, DateTimeKind.Local).AddTicks(5105) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 17, 13, 59, 41, 974, DateTimeKind.Local).AddTicks(4041), new DateTime(2026, 3, 17, 13, 59, 41, 974, DateTimeKind.Local).AddTicks(4052) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 17, 13, 59, 41, 974, DateTimeKind.Local).AddTicks(4053), new DateTime(2026, 3, 17, 13, 59, 41, 974, DateTimeKind.Local).AddTicks(4053) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 17, 13, 59, 41, 974, DateTimeKind.Local).AddTicks(4055), new DateTime(2026, 3, 17, 13, 59, 41, 974, DateTimeKind.Local).AddTicks(4055) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 17, 13, 59, 41, 974, DateTimeKind.Local).AddTicks(4056), new DateTime(2026, 3, 17, 13, 59, 41, 974, DateTimeKind.Local).AddTicks(4056) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 17, 13, 59, 41, 974, DateTimeKind.Local).AddTicks(4141), new DateTime(2026, 3, 17, 13, 59, 41, 974, DateTimeKind.Local).AddTicks(4141) });

            migrationBuilder.Sql(@"
                IF OBJECT_ID('dbo.Sp_EmployeesAttendanceReport', 'P') IS NOT NULL
                DROP PROCEDURE dbo.Sp_EmployeesAttendanceReport;
            ");
        }
    }
}
