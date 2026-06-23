# AGENTS.md

This folder contains the ASP.NET Core backend for the diploma project.

Project topic:

> “Axborot resurs markazidagi kitoblarning onlayn reytingini shakllantiruvchi axborot tizimini ishlab chiqish”

The backend provides a REST API for listing books, searching books, viewing book details, submitting ratings, and calculating book rating statistics.

## Main stack

Use this stack unless the user explicitly changes it:

* ASP.NET Core Web API
* C#
* .NET 10
* PostgreSQL
* Entity Framework Core
* Npgsql EF Core provider
* Meilisearch-backed search through backend abstraction
* PostgreSQL fallback search when configured
* REST API
* Swagger/OpenAPI for development
* xUnit for focused backend tests

Prefer a simple, clear, diploma-friendly architecture over enterprise overengineering.

## Current implementation status

The initial backend skeleton and core API have been implemented.

Current solution:

```text
backend/
  BookRatingSystem.sln
  .gitignore
  src/
    BookRatingSystem.Api/
    BookRatingSystem.Application/
    BookRatingSystem.Domain/
    BookRatingSystem.Infrastructure/
  tests/
    BookRatingSystem.Tests/
```

Implemented:

* `Book` and `BookRating` domain entities;
* `BookStatus` domain enum with `New`, `Banned`, and `Verified` statuses;
* `User` domain entity with `is_admin` and `profile_picture_url` database fields;
* EF Core `BookRatingDbContext`;
* explicit EF Core entity configurations;
* initial PostgreSQL migration;
* real PostgreSQL-backed data flow for books, ratings, users, and admin access;
* REST endpoints for public book list/details/search, admin book list/create/update/delete, ratings, register, login, admin users, admin ratings, and admin dashboard;
* simple JWT authentication;
* authorization policies for admin-only book management and authenticated rating submission;
* Meilisearch search behind `IBookSearchService`;
* book indexing behind `IBookIndexingService`;
* PostgreSQL fallback search through `PostgresBookSearchService`;
* CORS for `http://localhost:5173`;
* Swagger UI in development;
* basic application service and controller tests.

Out of current scope:

* borrowing, reservation, inventory, payment, notification, or user-role modules.

Do not implement out-of-scope library workflows unless the user explicitly asks.

## Backend goals

Maintain and extend the backend for a book rating information system.

Core features:

* store books;
* list books;
* get book details;
* create, update, and delete books;
* keep book visibility controlled by `BookStatus` (`New`, `Banned`, `Verified`);
* search books through Meilisearch, with PostgreSQL fallback when configured;
* submit book rating;
* register and login users with JWT authentication;
* restrict book create/update/delete endpoints to admin users;
* calculate average rating;
* count ratings;
* expose clean API endpoints for frontend integration.

The system is primarily about online book rating for an information resource center.

Do not add unrelated library workflows unless explicitly requested.

## Project responsibilities

### BookRatingSystem.Api

Contains:

* controllers;
* request contracts;
* dependency injection setup;
* middleware;
* Swagger/OpenAPI setup;
* CORS configuration;
* API validation behavior.

Current important files:

```text
src/BookRatingSystem.Api/Program.cs
src/BookRatingSystem.Api/Controllers/BooksController.cs
src/BookRatingSystem.Api/Contracts/CreateBookRatingRequest.cs
src/BookRatingSystem.Api/appsettings.json
```

### BookRatingSystem.Application

Contains:

* use cases;
* service interfaces;
* application services;
* DTOs;
* mapping logic;
* abstractions used by infrastructure.

Current important files:

```text
src/BookRatingSystem.Application/Books/BookService.cs
src/BookRatingSystem.Application/Books/IBookService.cs
src/BookRatingSystem.Application/Books/Dtos/
src/BookRatingSystem.Application/Abstractions/
```

### BookRatingSystem.Domain

Contains:

* core entities;
* domain rules;
* domain exceptions.

Current important files:

```text
src/BookRatingSystem.Domain/Entities/Book.cs
src/BookRatingSystem.Domain/Entities/BookRating.cs
src/BookRatingSystem.Domain/Exceptions/InvalidBookRatingException.cs
```

Keep domain simple and understandable.

### BookRatingSystem.Infrastructure

Contains:

* EF Core `DbContext`;
* entity configurations;
* EF Core repository implementation;
* PostgreSQL search implementation;
* time/system infrastructure;
* database migrations.

Current important files:

```text
src/BookRatingSystem.Infrastructure/Persistence/BookRatingDbContext.cs
src/BookRatingSystem.Infrastructure/Persistence/Configurations/
src/BookRatingSystem.Infrastructure/Persistence/Migrations/
src/BookRatingSystem.Infrastructure/Repositories/EfBookRepository.cs
src/BookRatingSystem.Infrastructure/Search/PostgresBookSearchService.cs
```

## Domain model

Use the current domain model unless the user changes it.

### Book

Represents a book in the information resource center.

Current fields:

```text
Id
Title
Author
Category
Description
PublishedYear
CoverImageUrl
Status
CreatedAt
UpdatedAt
Ratings
```

### BookRating

Represents a rating submitted for a book.

Current fields:

```text
Id
BookId
UserId
Value
Comment
CreatedAt
Book
User
```

Rating value must be from 1 to 5.

Comment length is limited to 500 characters.

A book can have many ratings.

Each rating belongs to one user through `BookRating.UserId`.

`averageRating` and `ratingsCount` are calculated from ratings instead of stored directly.

### User

Represents an authenticated API user.

Current fields:

```text
Id
Username
Email
PasswordHash
ProfilePictureUrl
IsAdmin
CreatedAt
Ratings
```

## Database rules

Use PostgreSQL.

Use EF Core migrations.

Use explicit entity configurations.

Current tables:

```text
books
book_ratings
users
```

Current indexes:

```text
IX_books_Title
IX_books_Author
IX_books_Category
IX_book_ratings_BookId
IX_book_ratings_UserId
IX_users_Email
IX_users_Username
```

Current migrations:

```text
20260622185401_InitialCreate
20260623084940_AddUsersAndJwtAuth
20260623090221_AddUserProfilePicturesAndRatingUsers
20260623124430_AddBookStatus
```

Do not rely on fake, mock, dummy, or demo seed data for backend behavior.

The backend must be written as production-oriented code that works with real PostgreSQL data, real API requests, and real authenticated users. If initial data is needed, use an intentional, clearly documented production-safe import or admin-created records rather than fake development users, fake ratings, or placeholder book datasets.

Do not hardcode production credentials.

Use configuration from `appsettings.json`, user secrets, or environment variables. The checked-in PostgreSQL connection string is a local development placeholder only.

## API endpoints

Implemented endpoints:

```text
GET    /api/books
GET    /api/books/{id}
GET    /api/books/search?q=...
POST   /api/auth/register
POST   /api/auth/login
GET    /api/admin/books           AdminOnly policy required
GET    /api/admin/users           AdminOnly policy required
GET    /api/admin/ratings         AdminOnly policy required
GET    /api/admin/dashboard       AdminOnly policy required
POST   /api/books                 AdminOnly policy required
PUT    /api/books/{id}            AdminOnly policy required
DELETE /api/books/{id}            AdminOnly policy required
POST   /api/books/{id}/ratings    AuthenticatedUser policy required
```

Public `GET /api/books`, `GET /api/books/{id}`, and `GET /api/books/search` return only `Verified` books. Admin book endpoints return all statuses.

Do not add extra authentication-protected admin behavior beyond the implemented dashboard/books/users/ratings scope unless explicitly requested.

## API response expectations

Book list items include:

```text
id
title
author
category
coverImageUrl
averageRating
ratingsCount
status
```

Book details include:

```text
id
title
author
category
description
publishedYear
coverImageUrl
averageRating
ratingsCount
status
recentRatings
```

Recent rating items include:

```text
id
bookId
userId
value
comment
createdAt
```

Admin dashboard includes total books, total users, total ratings, books added in range, ratings added in range, average rating in range, and recent ratings for the selected date range.

Rating submission request:

```json
{
  "value": 5,
  "comment": "Juda foydali kitob"
}
```

Validate rating value from 1 to 5.

Reject invalid requests with clear validation errors.

## Search implementation

Current implementation:

* `IBookSearchService` is defined in Application;
* `IBookIndexingService` is defined in Application;
* `MeilisearchBookSearchService` is defined in Infrastructure;
* `MeilisearchBookIndexingService` is defined in Infrastructure;
* `PostgresBookSearchService` is defined in Infrastructure;
* Meilisearch search checks title, author, category, and description;
* PostgreSQL fallback search uses EF Core query with `EF.Functions.ILike`;
* search checks title, author, and category;
* empty query is rejected at the API boundary.

Book index synchronization:

* create and update save to PostgreSQL, then call `IBookIndexingService.IndexBookAsync`;
* delete saves the PostgreSQL delete, then calls `IBookIndexingService.DeleteBookAsync`;
* rating submission saves the rating, then re-indexes the book so rating statistics in search stay current;
* Meilisearch sync failures must be logged and must not roll back successful PostgreSQL saves.

Keep Meilisearch credentials only in backend configuration. Never expose admin/master keys to frontend.

## CORS

Local frontend origin:

```text
http://localhost:5173
```

CORS is configured in `Program.cs` through a named policy.

Keep CORS configuration explicit.

Do not allow all origins in a final/demo configuration unless the user explicitly accepts it.

## Swagger/OpenAPI

Swagger/OpenAPI is enabled in development.

The API should remain easy to test from Swagger UI.

Use meaningful endpoint names, request DTOs, and response DTOs.

## Data and seeding

Do not add fake development seed data, mock datasets, or hardcoded demo records to EF Core configurations or migrations.

If the application needs starting data, it must come from a real source approved for the project, an explicit import process, or records created through the admin API.

Do not seed copyrighted book descriptions or long copied texts.

If real initial data requirements change, create or update migrations intentionally and verify the generated migration. Keep credentials and environment-specific data outside source control.

## Error handling

The API should not return raw exception details in normal responses.

Use clear errors for:

* book not found;
* invalid rating value;
* empty search query;
* database errors.

Use ASP.NET Core `ProblemDetails`-style responses where practical.

## Validation

Validate incoming requests.

Current minimum rules:

* book title is required;
* author is required;
* rating value must be between 1 and 5;
* rating comment length is limited.

Prefer simple validation first.

Do not introduce heavy validation libraries unless needed.

## Coding style

Use modern C#.

Prefer:

* async/await;
* cancellation tokens;
* dependency injection;
* nullable reference types;
* clear DTOs;
* small services;
* explicit names;
* simple readable code.

Avoid:

* unnecessary abstractions;
* generic repository pattern unless it adds real value;
* complex CQRS/MediatR setup for this project stage;
* hidden magic;
* huge controllers;
* business logic directly inside endpoints;
* untyped or dynamic data structures.

## Architecture rule

Keep the architecture understandable for a diploma thesis.

Every major part should be easy to explain:

* API layer receives requests;
* Application layer handles use cases;
* Infrastructure layer works with database/search;
* Domain layer contains core entities and rules.

Do not overcomplicate the project just to look advanced.

## Frontend compatibility

The backend must remain easy to connect to the existing React Vite frontend.

Keep response shapes close to frontend types.

Do not break frontend expectations without reporting it.

Do not modify frontend code from backend tasks unless the user explicitly asks for frontend integration.

## Testing

Current tests:

```text
tests/BookRatingSystem.Tests/BookServiceTests.cs
```

Expected commands from `src/backend`:

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/BookRatingSystem.Api
```

Migration command:

```bash
dotnet ef migrations add <MigrationName> --project src/BookRatingSystem.Infrastructure/BookRatingSystem.Infrastructure.csproj --startup-project src/BookRatingSystem.Api/BookRatingSystem.Api.csproj --output-dir Persistence/Migrations
```

Database update command:

```bash
dotnet ef database update --project src/BookRatingSystem.Infrastructure/BookRatingSystem.Infrastructure.csproj --startup-project src/BookRatingSystem.Api/BookRatingSystem.Api.csproj
```

Do not claim tests exist or pass unless `dotnet test` has been run successfully.

## Docker

Do not create Docker setup unless explicitly requested.

If Docker is requested later, keep it simple:

* backend container;
* PostgreSQL container;
* optional Meilisearch container later.

## Secrets and configuration

Do not commit real secrets.

Allowed local placeholder example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=book_rating_db;Username=postgres;Password=postgres"
  }
}
```

Real credentials should be changed outside source control through user secrets, environment variables, or deployment configuration.

## Generated files

Do not manually edit generated dependency/build output:

```text
bin/
obj/
TestResults/
```

These paths are ignored by the backend-local `.gitignore`.

## Diploma compatibility

The backend implementation must support writing the thesis practical chapter.

When adding features, keep them explainable using this structure:

1. Purpose of the module.
2. Used technologies.
3. Data model.
4. API flow.
5. Result achieved.

Prefer simple, demonstrable features over unfinished advanced features.

Do not invent implemented features in thesis text. If a feature is planned but not implemented, mark it as planned or `TODO:`.

## Work behavior

Before changing files, inspect the existing project.

Make small, focused, reviewable changes.

Do not delete existing files unless clearly unnecessary and explain why.

Do not rewrite working frontend code from backend tasks.

After completing work, respond briefly:

```text
Bajarildi:
- ...

Oʻzgargan fayllar:
- ...

Eʼtibor kerak:
- ...
```

Do not write long explanations unless the user asks.

## Hard rules

Do not invent implemented features.

Do not claim search engine integration exists before it is implemented.

Do not claim tests exist before they are written.

Do not claim the system is secure, optimized, deployed, or production-ready unless the code proves it.

Do not expand authentication, admin panel, borrowing, reservation, inventory, payment, notification, or user-role modules beyond the implemented scope unless explicitly requested.

Keep the project aligned with the diploma topic: online book rating.
