using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookRatingSystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBookStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "books",
                type: "text",
                nullable: false,
                defaultValue: "New");

            migrationBuilder.UpdateData(
                table: "books",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "status",
                value: "Verified");

            migrationBuilder.UpdateData(
                table: "books",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "status",
                value: "Verified");

            migrationBuilder.UpdateData(
                table: "books",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "status",
                value: "Verified");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "books");
        }
    }
}
