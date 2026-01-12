using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace arna.HRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addFestivalHolidayTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FestivalHoliday",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FestivalName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FestivalHoliday", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 9, 19, 49, 6, 682, DateTimeKind.Local).AddTicks(5113), new DateTime(2026, 1, 9, 19, 49, 6, 682, DateTimeKind.Local).AddTicks(5113) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 9, 19, 49, 6, 682, DateTimeKind.Local).AddTicks(5115), new DateTime(2026, 1, 9, 19, 49, 6, 682, DateTimeKind.Local).AddTicks(5116) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 9, 19, 49, 6, 682, DateTimeKind.Local).AddTicks(5117), new DateTime(2026, 1, 9, 19, 49, 6, 682, DateTimeKind.Local).AddTicks(5117) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 9, 19, 49, 6, 682, DateTimeKind.Local).AddTicks(5118), new DateTime(2026, 1, 9, 19, 49, 6, 682, DateTimeKind.Local).AddTicks(5118) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 9, 19, 49, 6, 682, DateTimeKind.Local).AddTicks(4467), new DateTime(2026, 1, 9, 19, 49, 6, 682, DateTimeKind.Local).AddTicks(4481) });

            migrationBuilder.CreateIndex(
                name: "IX_FestivalHoliday_Date",
                table: "FestivalHoliday",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_FestivalHoliday_FestivalName",
                table: "FestivalHoliday",
                column: "FestivalName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FestivalHoliday");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 7, 20, 8, 9, 154, DateTimeKind.Local).AddTicks(2046), new DateTime(2026, 1, 7, 20, 8, 9, 154, DateTimeKind.Local).AddTicks(2048) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 7, 20, 8, 9, 154, DateTimeKind.Local).AddTicks(2051), new DateTime(2026, 1, 7, 20, 8, 9, 154, DateTimeKind.Local).AddTicks(2051) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 7, 20, 8, 9, 154, DateTimeKind.Local).AddTicks(2055), new DateTime(2026, 1, 7, 20, 8, 9, 154, DateTimeKind.Local).AddTicks(2056) });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 7, 20, 8, 9, 154, DateTimeKind.Local).AddTicks(2057), new DateTime(2026, 1, 7, 20, 8, 9, 154, DateTimeKind.Local).AddTicks(2057) });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedBy", "UpdatedBy" },
                values: new object[] { new DateTime(2026, 1, 7, 20, 8, 9, 154, DateTimeKind.Local).AddTicks(1057), new DateTime(2026, 1, 7, 20, 8, 9, 154, DateTimeKind.Local).AddTicks(1067) });
        }
    }
}
