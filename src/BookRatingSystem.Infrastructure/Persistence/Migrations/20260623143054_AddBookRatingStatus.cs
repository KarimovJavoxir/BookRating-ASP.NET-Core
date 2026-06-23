using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookRatingSystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBookRatingStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ban_reason",
                table: "book_ratings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "book_ratings",
                type: "text",
                nullable: false,
                defaultValue: "New");

            migrationBuilder.UpdateData(
                table: "book_ratings",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-1111-1111-1111-111111111111"),
                column: "ban_reason",
                value: null);

            migrationBuilder.UpdateData(
                table: "book_ratings",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-2222-2222-2222-222222222222"),
                column: "ban_reason",
                value: null);

            migrationBuilder.UpdateData(
                table: "book_ratings",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-3333-3333-3333-333333333333"),
                column: "ban_reason",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ban_reason",
                table: "book_ratings");

            migrationBuilder.DropColumn(
                name: "status",
                table: "book_ratings");
        }
    }
}
