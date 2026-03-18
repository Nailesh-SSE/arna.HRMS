using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace arna.HRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Rename_SP_DailyAttendanceReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop old SP safely
            migrationBuilder.Sql(@"
                IF OBJECT_ID('dbo.Sp_AttendanceReport', 'P') IS NOT NULL
                DROP PROCEDURE dbo.Sp_AttendanceReport;
            ");

            // Drop NEW SP (safety)
            migrationBuilder.Sql(@"
                IF OBJECT_ID('dbo.Sp_DailyAttendanceReport', 'P') IS NOT NULL
                DROP PROCEDURE dbo.Sp_DailyAttendanceReport;
            ");

            // Create new SP
            migrationBuilder.Sql(@"
                 CREATE PROCEDURE dbo.Sp_DailyAttendanceReport
                 (
                     @Year INT = NULL,
                     @Month INT = NULL,
                     @EmployeeId INT = NULL,
                     @StatusId INT = NULL,
                     @DepartmentId INT = NULL,
                     @DeviceId INT = NULL,
                     @FromDate DATE = NULL,
                     @ToDate DATE = NULL
                 )
                 AS
                 BEGIN
                     SET NOCOUNT ON;

                     SELECT
                         a.EmployeeId,
                         e.EmployeeNumber,
                         CONCAT(e.FirstName, ' ', e.LastName) AS EmployeeName,
                         d.DepartmentName AS Department,

                         CAST(a.Date AS DATE) AS AttendanceDate,
                         DATENAME(WEEKDAY, CAST(a.Date AS DATE)) AS Day,

                         CAST(MIN(a.ClockIn) AS TIME) AS ClockIn,
                         CAST(MAX(a.ClockOut) AS TIME) AS ClockOut,

                         CAST(
                             DATEADD(
                                 SECOND,
                                SUM(
                                    CASE 
                                        WHEN a.TotalHours IS NOT NULL 
                                        THEN DATEDIFF(SECOND,0,a.TotalHours)
                                    END
                                    ),
                                 0
                             ) AS TIME
                         ) AS WorkingHours,

                         CAST(
                            DATEADD(
                                SECOND,
                                CASE 
                                    WHEN MIN(a.ClockIn) IS NULL OR MAX(a.ClockOut) IS NULL
                                    THEN NULL
                                    ELSE DATEDIFF(SECOND, MIN(a.ClockIn), MAX(a.ClockOut))
                                END,
                                0
                            ) AS TIME
                        ) AS TotalHours ,

                         CAST(
                            DATEADD(
                                SECOND,
                                CASE 
                                    WHEN MIN(a.ClockIn) IS NULL OR MAX(a.ClockOut) IS NULL
                                    THEN NULL
                                    WHEN DATEDIFF(SECOND, MIN(a.ClockIn), MAX(a.ClockOut))
                                         - SUM(
                                             CASE 
                                                 WHEN a.TotalHours IS NOT NULL 
                                                 THEN DATEDIFF(SECOND,0,a.TotalHours)
                                             END
                                         ) < 0
                                    THEN 0
                                    ELSE
                                        DATEDIFF(SECOND, MIN(a.ClockIn), MAX(a.ClockOut))
                                        - SUM(
                                            CASE 
                                                WHEN a.TotalHours IS NOT NULL 
                                                THEN DATEDIFF(SECOND,0,a.TotalHours)
                                            END
                                        )
                                END,
                                0
                            ) AS TIME
                        ) AS BreakDuration,

                         MAX(a.Latitude) AS Latitude,
                         MAX(a.Longitude) AS Longitude,

                         STUFF(
                             (
                                 SELECT DISTINCT ', ' + CAST(a2.DeviceId AS NVARCHAR(10))
                                 FROM Attendances a2
                                 WHERE 
                                     a2.EmployeeId = a.EmployeeId
                                     AND CAST(a2.Date AS DATE) = CAST(a.Date AS DATE)
                                     AND a2.DeviceId IS NOT NULL
                                 FOR XML PATH(''), TYPE
                             ).value('.', 'NVARCHAR(MAX)'), 1, 2, ''
                         ) AS Device,

                         MAX(a.StatusId) AS AttendanceStatus

                     FROM Attendances a
                     INNER JOIN Employees e ON e.Id = a.EmployeeId
                     LEFT JOIN Departments d ON d.Id = e.DepartmentId

                     WHERE
                         a.IsActive = 1
                         AND a.IsDeleted = 0
                         AND e.IsActive = 1
                         AND e.IsDeleted = 0

                         AND (@Year IS NULL OR YEAR(a.Date) = @Year)
                         AND (@Month IS NULL OR MONTH(a.Date) = @Month)
                         AND (@EmployeeId IS NULL OR a.EmployeeId = @EmployeeId)
                         AND (@StatusId IS NULL OR a.StatusId = @StatusId)
                         AND (@DepartmentId IS NULL OR d.Id = @DepartmentId)
                         AND (@DeviceId IS NULL OR a.DeviceId = @DeviceId)
                         AND (@FromDate IS NULL OR a.Date >= @FromDate)
                         AND (@ToDate IS NULL OR a.Date <= @ToDate)

                     GROUP BY
                         a.EmployeeId,
                         e.EmployeeNumber,
                         e.FirstName,
                         e.LastName,
                         d.DepartmentName,
                         CAST(a.Date AS DATE)

                     ORDER BY
                         AttendanceDate,
                         EmployeeName;
                 END
             ");
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 13, 16, 41, 5, 277, DateTimeKind.Local).AddTicks(4994), new DateTime(2026, 3, 13, 16, 41, 5, 277, DateTimeKind.Local).AddTicks(4995) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 13, 16, 41, 5, 277, DateTimeKind.Local).AddTicks(4996), new DateTime(2026, 3, 13, 16, 41, 5, 277, DateTimeKind.Local).AddTicks(4997) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 13, 16, 41, 5, 277, DateTimeKind.Local).AddTicks(4998), new DateTime(2026, 3, 13, 16, 41, 5, 277, DateTimeKind.Local).AddTicks(4998) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 13, 16, 41, 5, 277, DateTimeKind.Local).AddTicks(4249), new DateTime(2026, 3, 13, 16, 41, 5, 277, DateTimeKind.Local).AddTicks(4259) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 13, 16, 41, 5, 277, DateTimeKind.Local).AddTicks(4260), new DateTime(2026, 3, 13, 16, 41, 5, 277, DateTimeKind.Local).AddTicks(4261) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 13, 16, 41, 5, 277, DateTimeKind.Local).AddTicks(4262), new DateTime(2026, 3, 13, 16, 41, 5, 277, DateTimeKind.Local).AddTicks(4262) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 13, 16, 41, 5, 277, DateTimeKind.Local).AddTicks(4263), new DateTime(2026, 3, 13, 16, 41, 5, 277, DateTimeKind.Local).AddTicks(4263) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 13, 16, 41, 5, 277, DateTimeKind.Local).AddTicks(4353), new DateTime(2026, 3, 13, 16, 41, 5, 277, DateTimeKind.Local).AddTicks(4353) });

            migrationBuilder.Sql(@"
                IF OBJECT_ID('dbo.Sp_DailyAttendanceReport', 'P') IS NOT NULL
                DROP PROCEDURE dbo.Sp_DailyAttendanceReport;
            ");
        }
    }
}
