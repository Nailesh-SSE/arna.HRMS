using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace arna.HRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLatitudeLongitudeToAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Attendances",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Attendances",
                type: "float",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 28, 15, 28, 12, 837, DateTimeKind.Local).AddTicks(8519), new DateTime(2026, 1, 28, 15, 28, 12, 837, DateTimeKind.Local).AddTicks(8520) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 28, 15, 28, 12, 837, DateTimeKind.Local).AddTicks(8522), new DateTime(2026, 1, 28, 15, 28, 12, 837, DateTimeKind.Local).AddTicks(8522) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 28, 15, 28, 12, 837, DateTimeKind.Local).AddTicks(8523), new DateTime(2026, 1, 28, 15, 28, 12, 837, DateTimeKind.Local).AddTicks(8524) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 28, 15, 28, 12, 837, DateTimeKind.Local).AddTicks(8525), new DateTime(2026, 1, 28, 15, 28, 12, 837, DateTimeKind.Local).AddTicks(8525) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 28, 15, 28, 12, 837, DateTimeKind.Local).AddTicks(8541), new DateTime(2026, 1, 28, 15, 28, 12, 837, DateTimeKind.Local).AddTicks(8541) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 28, 15, 28, 12, 837, DateTimeKind.Local).AddTicks(8542), new DateTime(2026, 1, 28, 15, 28, 12, 837, DateTimeKind.Local).AddTicks(8543) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 28, 15, 28, 12, 837, DateTimeKind.Local).AddTicks(8544), new DateTime(2026, 1, 28, 15, 28, 12, 837, DateTimeKind.Local).AddTicks(8544) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 28, 15, 28, 12, 837, DateTimeKind.Local).AddTicks(8545), new DateTime(2026, 1, 28, 15, 28, 12, 837, DateTimeKind.Local).AddTicks(8545) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 28, 15, 28, 12, 837, DateTimeKind.Local).AddTicks(8546), new DateTime(2026, 1, 28, 15, 28, 12, 837, DateTimeKind.Local).AddTicks(8546) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 28, 15, 28, 12, 837, DateTimeKind.Local).AddTicks(7913), new DateTime(2026, 1, 28, 15, 28, 12, 837, DateTimeKind.Local).AddTicks(7923) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Attendances");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(7606), new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(7607) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(7609), new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(7609) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(7610), new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(7611) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(7612), new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(7612) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(7627), new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(7627) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(7628), new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(7629) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(7630), new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(7630) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(7631), new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(7631) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(7632), new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(7632) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(6959), new DateTime(2026, 1, 22, 12, 57, 23, 480, DateTimeKind.Local).AddTicks(6968) });
        }
    }
}
