using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace arna.HRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Create_Sp_AttendanceReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                IF OBJECT_ID('dbo.Sp_AttendanceReport', 'P') IS NOT NULL
                DROP PROCEDURE dbo.Sp_AttendanceReport; ");      

            migrationBuilder.Sql(@"
                CREATE PROCEDURE dbo.Sp_AttendanceReport

                (
                    @Year INT = NULL,
                    @Month INT = NULL,
                    @EmployeeId INT = NULL,
                    @StatusId INT = NULL,
                    @DepartmentId INT = NULL,
                    @DeviceId INT = NULL
                )
                AS
                BEGIN
                    SET NOCOUNT ON;
	
	                SELECT

		                a.EmployeeId,

		                e.EmployeeNumber,

		                CONCAT(e.FirstName,' ',e.LastName) AS EmployeeName,

		                d.DepartmentName AS Department,

		                CAST(a.Date AS DATE) AS AttendanceDate,

		                DATENAME(WEEKDAY, CAST(a.Date AS DATE)) AS Day,

		                CAST(MIN(a.ClockIn) AS TIME) AS ClockIn,
		                CAST(MAX(a.ClockOut) AS TIME) AS ClockOut,

		                /* WORKING HOURS (sum of recorded work sessions) */

		                CAST(
			                DATEADD(
				                SECOND,
				                SUM(
					                CASE 
						                WHEN a.TotalHours IS NOT NULL 
						                THEN DATEDIFF(SECOND,0,a.TotalHours)
						                ELSE 0
					                END
				                ),
				                0
			                ) AS TIME
		                ) AS WorkingHours,


		                /* TOTAL HOURS (first in → last out) */

		                CAST(
			                DATEADD(
				                SECOND,
				                DATEDIFF(SECOND, MIN(a.ClockIn), MAX(a.ClockOut)),
				                0
			                ) AS TIME
		                ) AS TotalHours,


		                /* BREAK = total - working */

		                CAST(
			                DATEADD(
				                SECOND,
				                CASE 
					                WHEN DATEDIFF(SECOND, MIN(a.ClockIn), MAX(a.ClockOut))
						                 - SUM(CASE WHEN a.TotalHours IS NOT NULL 
									                THEN DATEDIFF(SECOND,0,a.TotalHours) 
									                ELSE 0 END) < 0
					                THEN 0
					                ELSE
						                DATEDIFF(SECOND, MIN(a.ClockIn), MAX(a.ClockOut))
						                - SUM(CASE WHEN a.TotalHours IS NOT NULL 
								                   THEN DATEDIFF(SECOND,0,a.TotalHours) 
								                   ELSE 0 END)
				                END,
				                0
			                ) AS TIME
		                ) AS BreakDuration,


		                MAX(a.Latitude) AS Latitude,
		                MAX(a.Longitude) AS Longitude,


		                /* DEVICE LIST */

		                STUFF(
		                (
			                SELECT DISTINCT ', ' + CAST(a2.DeviceId AS NVARCHAR(10))
			                FROM Attendances a2
			                WHERE
				                a2.EmployeeId = a.EmployeeId
				                AND CAST(a2.Date AS DATE) = CAST(a.Date AS DATE)
				                AND a2.DeviceId IS NOT NULL
			                FOR XML PATH(''), TYPE
		                ).value('.', 'NVARCHAR(MAX)'),1,2,'') AS Device,

		                /* STATUS */

		                MAX(a.StatusId) AS AttendanceStatus

	                FROM Attendances a

	                INNER JOIN Employees e
		                ON e.Id = a.EmployeeId

	                LEFT JOIN Departments d
		                ON d.Id = e.DepartmentId

	                WHERE

	                a.IsActive = 1
	                AND a.IsDeleted = 0

	                AND e.IsActive = 1
	                AND e.IsDeleted = 0

	                -- Year filter
	                AND (@Year IS NULL OR YEAR(a.Date) = @Year)

	                -- Month filter
	                AND (@Month IS NULL OR MONTH(a.Date) = @Month)

	                -- Employee filter
	                AND (@EmployeeId IS NULL OR a.EmployeeId = @EmployeeId)

	                -- Status filter
	                AND (@StatusId IS NULL OR a.StatusId = @StatusId)

	                -- Department filter
	                AND (@DepartmentId IS NULL OR d.Id = @DepartmentID)

	                -- Device filter
	                AND (@DeviceId IS NULL OR a.DeviceId = @DeviceId)

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

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 13, 15, 58, 30, 358, DateTimeKind.Local).AddTicks(4789), new DateTime(2026, 3, 13, 15, 58, 30, 358, DateTimeKind.Local).AddTicks(4789) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 13, 15, 58, 30, 358, DateTimeKind.Local).AddTicks(4792), new DateTime(2026, 3, 13, 15, 58, 30, 358, DateTimeKind.Local).AddTicks(4792) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 13, 15, 58, 30, 358, DateTimeKind.Local).AddTicks(4793), new DateTime(2026, 3, 13, 15, 58, 30, 358, DateTimeKind.Local).AddTicks(4794) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 13, 15, 58, 30, 358, DateTimeKind.Local).AddTicks(3945), new DateTime(2026, 3, 13, 15, 58, 30, 358, DateTimeKind.Local).AddTicks(3956) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 13, 15, 58, 30, 358, DateTimeKind.Local).AddTicks(3957), new DateTime(2026, 3, 13, 15, 58, 30, 358, DateTimeKind.Local).AddTicks(3958) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 13, 15, 58, 30, 358, DateTimeKind.Local).AddTicks(3959), new DateTime(2026, 3, 13, 15, 58, 30, 358, DateTimeKind.Local).AddTicks(3959) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 13, 15, 58, 30, 358, DateTimeKind.Local).AddTicks(3960), new DateTime(2026, 3, 13, 15, 58, 30, 358, DateTimeKind.Local).AddTicks(3960) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 13, 15, 58, 30, 358, DateTimeKind.Local).AddTicks(4060), new DateTime(2026, 3, 13, 15, 58, 30, 358, DateTimeKind.Local).AddTicks(4061) });

            migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS dbo.Sp_AttendanceReport;");
        }
    }
}
