using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BookRatingSystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "books",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Author = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PublishedYear = table.Column<int>(type: "integer", nullable: true),
                    CoverImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_books", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "book_ratings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_book_ratings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_book_ratings_books_BookId",
                        column: x => x.BookId,
                        principalTable: "books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "books",
                columns: new[] { "Id", "Author", "Category", "CoverImageUrl", "CreatedAt", "Description", "PublishedYear", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "A. Karimov", "Dasturlash", null, new DateTimeOffset(new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Algoritm tushunchasi, saralash va qidiruv usullari haqida oʻquv material.", 2024, "Algoritmlar asoslari", new DateTimeOffset(new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "D. Rahimov", "Database", null, new DateTimeOffset(new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Relatsion maʼlumotlar bazasi, SQL va loyihalash asoslari.", 2023, "Maʼlumotlar bazasi", new DateTimeOffset(new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "S. Aliyev", "Tarmoq", null, new DateTimeOffset(new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Kompyuter tarmoqlarining asosiy protokollari va amaliy qoʻllanilishi.", 2022, "Kompyuter tarmoqlari", new DateTimeOffset(new DateTime(2026, 6, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.InsertData(
                table: "book_ratings",
                columns: new[] { "Id", "BookId", "Comment", "CreatedAt", "Value" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-1111-1111-1111-111111111111"), new Guid("11111111-1111-1111-1111-111111111111"), "Mavzular sodda tushuntirilgan.", new DateTimeOffset(new DateTime(2026, 6, 22, 0, 10, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 5 },
                    { new Guid("aaaaaaaa-2222-2222-2222-222222222222"), new Guid("11111111-1111-1111-1111-111111111111"), "Amaliy misollar foydali.", new DateTimeOffset(new DateTime(2026, 6, 22, 0, 20, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 4 },
                    { new Guid("aaaaaaaa-3333-3333-3333-333333333333"), new Guid("22222222-2222-2222-2222-222222222222"), "Database loyihalash uchun qulay qoʻllanma.", new DateTimeOffset(new DateTime(2026, 6, 22, 0, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 5 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_book_ratings_BookId",
                table: "book_ratings",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_books_Author",
                table: "books",
                column: "Author");

            migrationBuilder.CreateIndex(
                name: "IX_books_Category",
                table: "books",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_books_Title",
                table: "books",
                column: "Title");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "book_ratings");

            migrationBuilder.DropTable(
                name: "books");
        }
    }
}
