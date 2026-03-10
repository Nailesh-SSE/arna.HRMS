using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace arna.HRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOfficeEmailToEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "OfficeEmail",
                table: "Employees",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 10, 15, 11, 15, 22, DateTimeKind.Local).AddTicks(8978), new DateTime(2026, 3, 10, 15, 11, 15, 22, DateTimeKind.Local).AddTicks(8989) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 10, 15, 11, 15, 22, DateTimeKind.Local).AddTicks(8994), new DateTime(2026, 3, 10, 15, 11, 15, 22, DateTimeKind.Local).AddTicks(8994) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 10, 15, 11, 15, 22, DateTimeKind.Local).AddTicks(8997), new DateTime(2026, 3, 10, 15, 11, 15, 22, DateTimeKind.Local).AddTicks(8997) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 10, 15, 11, 15, 22, DateTimeKind.Local).AddTicks(3655), new DateTime(2026, 3, 10, 15, 11, 15, 22, DateTimeKind.Local).AddTicks(3670) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 10, 15, 11, 15, 22, DateTimeKind.Local).AddTicks(3675), new DateTime(2026, 3, 10, 15, 11, 15, 22, DateTimeKind.Local).AddTicks(3675) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 10, 15, 11, 15, 22, DateTimeKind.Local).AddTicks(3677), new DateTime(2026, 3, 10, 15, 11, 15, 22, DateTimeKind.Local).AddTicks(3678) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 10, 15, 11, 15, 22, DateTimeKind.Local).AddTicks(3679), new DateTime(2026, 3, 10, 15, 11, 15, 22, DateTimeKind.Local).AddTicks(3680) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 3, 10, 15, 11, 15, 22, DateTimeKind.Local).AddTicks(3874), new DateTime(2026, 3, 10, 15, 11, 15, 22, DateTimeKind.Local).AddTicks(3875) });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_OfficeEmail",
                table: "Employees",
                column: "OfficeEmail",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Employees_OfficeEmail",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "OfficeEmail",
                table: "Employees");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 27, 14, 9, 36, 296, DateTimeKind.Local).AddTicks(4308), new DateTime(2026, 2, 27, 14, 9, 36, 296, DateTimeKind.Local).AddTicks(4308) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 27, 14, 9, 36, 296, DateTimeKind.Local).AddTicks(4313), new DateTime(2026, 2, 27, 14, 9, 36, 296, DateTimeKind.Local).AddTicks(4313) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 27, 14, 9, 36, 296, DateTimeKind.Local).AddTicks(4314), new DateTime(2026, 2, 27, 14, 9, 36, 296, DateTimeKind.Local).AddTicks(4314) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 27, 14, 9, 36, 296, DateTimeKind.Local).AddTicks(3618), new DateTime(2026, 2, 27, 14, 9, 36, 296, DateTimeKind.Local).AddTicks(3629) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 27, 14, 9, 36, 296, DateTimeKind.Local).AddTicks(3630), new DateTime(2026, 2, 27, 14, 9, 36, 296, DateTimeKind.Local).AddTicks(3630) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 27, 14, 9, 36, 296, DateTimeKind.Local).AddTicks(3631), new DateTime(2026, 2, 27, 14, 9, 36, 296, DateTimeKind.Local).AddTicks(3632) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 27, 14, 9, 36, 296, DateTimeKind.Local).AddTicks(3633), new DateTime(2026, 2, 27, 14, 9, 36, 296, DateTimeKind.Local).AddTicks(3633) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 2, 27, 14, 9, 36, 296, DateTimeKind.Local).AddTicks(3715), new DateTime(2026, 2, 27, 14, 9, 36, 296, DateTimeKind.Local).AddTicks(3716) });
        }
    }
}
