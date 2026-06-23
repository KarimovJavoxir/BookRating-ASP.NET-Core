using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BookRatingSystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersAndJwtAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    is_admin = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "Id", "CreatedAt", "Email", "is_admin", "PasswordHash", "Username" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), new DateTimeOffset(new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user01@bookrate.uz", false, "pbkdf2-sha256:100000:Ym9va3JhdGUtdXNlci0wMQ==:zSd6h5V1wdpG1ZYdtbgZAeCgizz+WHwKSbeVrQm9PG8=", "user01" },
                    { new Guid("10000000-0000-0000-0000-000000000002"), new DateTimeOffset(new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user02@bookrate.uz", false, "pbkdf2-sha256:100000:Ym9va3JhdGUtdXNlci0wMg==:cfJdJL9XutCfXCnLKAjefa+WYPQb/LqhFdwLWyzE8jQ=", "user02" },
                    { new Guid("10000000-0000-0000-0000-000000000003"), new DateTimeOffset(new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user03@bookrate.uz", false, "pbkdf2-sha256:100000:Ym9va3JhdGUtdXNlci0wMw==:cn9ONRzRx+NmxXa7f7SPv5CXYstD4tMSq/yjrhrCoMQ=", "user03" },
                    { new Guid("10000000-0000-0000-0000-000000000004"), new DateTimeOffset(new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user04@bookrate.uz", false, "pbkdf2-sha256:100000:Ym9va3JhdGUtdXNlci0wNA==:2SYUOW50hDBlp+z8iOG0ttGlr8wlKYgzYS4IuM2ArqQ=", "user04" },
                    { new Guid("10000000-0000-0000-0000-000000000005"), new DateTimeOffset(new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user05@bookrate.uz", false, "pbkdf2-sha256:100000:Ym9va3JhdGUtdXNlci0wNQ==:bzmJJZjJl2bSUBfkvFyHN1bBgPYPORpwKTGag2nFjtY=", "user05" },
                    { new Guid("10000000-0000-0000-0000-000000000006"), new DateTimeOffset(new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user06@bookrate.uz", false, "pbkdf2-sha256:100000:Ym9va3JhdGUtdXNlci0wNg==:cMb0sfq0hH8EYqDP+aqgjt8FiWfDwmdxnF68riL8rBU=", "user06" },
                    { new Guid("10000000-0000-0000-0000-000000000007"), new DateTimeOffset(new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user07@bookrate.uz", false, "pbkdf2-sha256:100000:Ym9va3JhdGUtdXNlci0wNw==:6/v/glazk6XiDjtj2kRLggc/xAGvdmlp3w+AaWuPvlw=", "user07" },
                    { new Guid("10000000-0000-0000-0000-000000000008"), new DateTimeOffset(new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user08@bookrate.uz", false, "pbkdf2-sha256:100000:Ym9va3JhdGUtdXNlci0wOA==:kpZ2X+KmGWrCwHYHrXresTrCUJBj7HGK1jCqdYklogU=", "user08" },
                    { new Guid("10000000-0000-0000-0000-000000000009"), new DateTimeOffset(new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user09@bookrate.uz", false, "pbkdf2-sha256:100000:Ym9va3JhdGUtdXNlci0wOQ==:JB8H1epXgYaa5SmwuES03kPtMvsTJ5Z44UwpVRsbIYE=", "user09" },
                    { new Guid("10000000-0000-0000-0000-000000000010"), new DateTimeOffset(new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user10@bookrate.uz", false, "pbkdf2-sha256:100000:Ym9va3JhdGUtdXNlci0xMA==:AhGFRTegNfBqz+VCkdZqf3d+9fidRfs12zfMVG49Z98=", "user10" },
                    { new Guid("10000000-0000-0000-0000-000000000011"), new DateTimeOffset(new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user11@bookrate.uz", false, "pbkdf2-sha256:100000:Ym9va3JhdGUtdXNlci0xMQ==:61LNsJWaJcT6JwsnpSWLeSIEJFBnhsEwiJOKykOLQw8=", "user11" },
                    { new Guid("10000000-0000-0000-0000-000000000012"), new DateTimeOffset(new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user12@bookrate.uz", false, "pbkdf2-sha256:100000:Ym9va3JhdGUtdXNlci0xMg==:htxKhihDN84cU4qpPhFoY/r6lMU7p/fJbf7XJgQVbmk=", "user12" },
                    { new Guid("10000000-0000-0000-0000-000000000013"), new DateTimeOffset(new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user13@bookrate.uz", false, "pbkdf2-sha256:100000:Ym9va3JhdGUtdXNlci0xMw==:neqv9wU7oH6M3dycNvbKnYrHtESdWmCz7FIgYJD5X+c=", "user13" },
                    { new Guid("10000000-0000-0000-0000-000000000014"), new DateTimeOffset(new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user14@bookrate.uz", false, "pbkdf2-sha256:100000:Ym9va3JhdGUtdXNlci0xNA==:bjF9JAucLgOUEKDpBGfz1NRzAuNj0kmoZ6S4RmoS36o=", "user14" },
                    { new Guid("10000000-0000-0000-0000-000000000015"), new DateTimeOffset(new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user15@bookrate.uz", false, "pbkdf2-sha256:100000:Ym9va3JhdGUtdXNlci0xNQ==:Q93EidFVLYD6Df01FA0drMsWY5+Hfw8H7N+mMQgWi68=", "user15" },
                    { new Guid("10000000-0000-0000-0000-000000000016"), new DateTimeOffset(new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user16@bookrate.uz", false, "pbkdf2-sha256:100000:Ym9va3JhdGUtdXNlci0xNg==:7cKp8HE+8ouf1PveJeUNbKdETAiuVvfO0WGwZBMf2yM=", "user16" },
                    { new Guid("10000000-0000-0000-0000-000000000017"), new DateTimeOffset(new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user17@bookrate.uz", false, "pbkdf2-sha256:100000:Ym9va3JhdGUtdXNlci0xNw==:8chrpinbAXrVWQH3DE5KsrQbieIOo2zM7xgy0EWq86w=", "user17" },
                    { new Guid("10000000-0000-0000-0000-000000000018"), new DateTimeOffset(new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user18@bookrate.uz", false, "pbkdf2-sha256:100000:Ym9va3JhdGUtdXNlci0xOA==:5c46WnwWp+bSSht2oAXwEJo3qoINB96lIY8c6PQaOoU=", "user18" },
                    { new Guid("10000000-0000-0000-0000-000000000019"), new DateTimeOffset(new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user19@bookrate.uz", false, "pbkdf2-sha256:100000:Ym9va3JhdGUtdXNlci0xOQ==:IRAoedGtpr0NBaoobPGy5yMw3XaYaw9EU19TcK7tiZ0=", "user19" },
                    { new Guid("10000000-0000-0000-0000-000000000020"), new DateTimeOffset(new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user20@bookrate.uz", false, "pbkdf2-sha256:100000:Ym9va3JhdGUtdXNlci0yMA==:5ktXe+iVYBAd0my8ohEId3hyX1aPh1aYSJirTJutipY=", "user20" },
                    { new Guid("20000000-0000-0000-0000-000000000001"), new DateTimeOffset(new DateTime(2026, 6, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "admin@bookrate.uz", true, "pbkdf2-sha256:100000:Ym9va3JhdGUtYWRtaW4tMDE=:eBcb6RyQ/zvA5gcKCLEytg/IOkowlvvXNQ8S5rqs4HQ=", "admin" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_Username",
                table: "users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
