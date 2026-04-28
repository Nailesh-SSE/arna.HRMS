using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace arna.HRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddForeignKeyInUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.RenameColumn(
                name: "Role",
                table: "Users",
                newName: "RoleId");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Code", "CreatedOn", "Description", "DepartmentName", "UpdatedOn" },
                values: new object[] { "IT", new DateTime(2026, 1, 29, 18, 55, 51, 759, DateTimeKind.Local).AddTicks(5600), "Manages IT infrastructure and software systems", "Information Technology", new DateTime(2026, 1, 29, 18, 55, 51, 759, DateTimeKind.Local).AddTicks(5606) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Code", "CreatedOn", "Description", "DepartmentName", "UpdatedOn" },
                values: new object[] { "QA", new DateTime(2026, 1, 29, 18, 55, 51, 759, DateTimeKind.Local).AddTicks(5609), "Tests software and ensures quality standards are met before release", "Quality Assurance", new DateTime(2026, 1, 29, 18, 55, 51, 759, DateTimeKind.Local).AddTicks(5610) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Code", "CreatedOn", "Description", "DepartmentName", "UpdatedOn" },
                values: new object[] { "ADMIN", new DateTime(2026, 1, 29, 18, 55, 51, 759, DateTimeKind.Local).AddTicks(5611), "Manages facilities, office administration, and general services", "Administration", new DateTime(2026, 1, 29, 18, 55, 51, 759, DateTimeKind.Local).AddTicks(5612) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 29, 18, 55, 51, 759, DateTimeKind.Local).AddTicks(2090), new DateTime(2026, 1, 29, 18, 55, 51, 759, DateTimeKind.Local).AddTicks(2100) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 29, 18, 55, 51, 759, DateTimeKind.Local).AddTicks(2102), new DateTime(2026, 1, 29, 18, 55, 51, 759, DateTimeKind.Local).AddTicks(2103) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedOn", "Description", "Name", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 29, 18, 55, 51, 759, DateTimeKind.Local).AddTicks(2104), "Manager role with team oversight", "Manager", new DateTime(2026, 1, 29, 18, 55, 51, 759, DateTimeKind.Local).AddTicks(2104) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedOn", "Description", "Name", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 29, 18, 55, 51, 759, DateTimeKind.Local).AddTicks(2105), "Standard employee role", "Employee", new DateTime(2026, 1, 29, 18, 55, 51, 759, DateTimeKind.Local).AddTicks(2105) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 29, 18, 55, 51, 759, DateTimeKind.Local).AddTicks(2177), new DateTime(2026, 1, 29, 18, 55, 51, 759, DateTimeKind.Local).AddTicks(2178) });

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_RoleId",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "Users",
                newName: "Role");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Code", "CreatedOn", "Description", "DepartmentName", "UpdatedOn" },
                values: new object[] { "HR", new DateTime(2026, 1, 28, 15, 28, 12, 837, DateTimeKind.Local).AddTicks(8519), "Handles recruitment, payroll, and employee relations", "Human Resources", new DateTime(2026, 1, 28, 15, 28, 12, 837, DateTimeKind.Local).AddTicks(8520) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Code", "CreatedOn", "Description", "DepartmentName", "UpdatedOn" },
                values: new object[] { "IT", new DateTime(2026, 1, 28, 15, 28, 12, 837, DateTimeKind.Local).AddTicks(8522), "Manages IT infrastructure and software systems", "Information Technology", new DateTime(2026, 1, 28, 15, 28, 12, 837, DateTimeKind.Local).AddTicks(8522) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Code", "CreatedOn", "Description", "DepartmentName", "UpdatedOn" },
                values: new object[] { "FIN", new DateTime(2026, 1, 28, 15, 28, 12, 837, DateTimeKind.Local).AddTicks(8523), "Responsible for accounting and financial management", "Finance", new DateTime(2026, 1, 28, 15, 28, 12, 837, DateTimeKind.Local).AddTicks(8524) });

            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "Id", "Code", "CreatedOn", "Description", "IsActive", "IsDeleted", "DepartmentName", "ParentDepartmentId", "UpdatedOn" },
                values: new object[] { 4, "ADMIN", new DateTime(2026, 1, 28, 15, 28, 12, 837, DateTimeKind.Local).AddTicks(8525), "Office administration and facilities management", true, false, "Administration", 4, new DateTime(2026, 1, 28, 15, 28, 12, 837, DateTimeKind.Local).AddTicks(8525) });

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
                columns: new[] { "CreatedOn", "Description", "Name", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 28, 15, 28, 12, 837, DateTimeKind.Local).AddTicks(8544), "Human Resources role", "HR", new DateTime(2026, 1, 28, 15, 28, 12, 837, DateTimeKind.Local).AddTicks(8544) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedOn", "Description", "Name", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 28, 15, 28, 12, 837, DateTimeKind.Local).AddTicks(8545), "Manager role with team oversight", "Manager", new DateTime(2026, 1, 28, 15, 28, 12, 837, DateTimeKind.Local).AddTicks(8545) });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedOn", "Description", "IsActive", "IsDeleted", "Name", "UpdatedOn" },
                values: new object[] { 5, new DateTime(2026, 1, 28, 15, 28, 12, 837, DateTimeKind.Local).AddTicks(8546), "Standard employee role", true, false, "Employee", new DateTime(2026, 1, 28, 15, 28, 12, 837, DateTimeKind.Local).AddTicks(8546) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedOn", "UpdatedOn" },
                values: new object[] { new DateTime(2026, 1, 28, 15, 28, 12, 837, DateTimeKind.Local).AddTicks(7913), new DateTime(2026, 1, 28, 15, 28, 12, 837, DateTimeKind.Local).AddTicks(7923) });
        }
    }
}
