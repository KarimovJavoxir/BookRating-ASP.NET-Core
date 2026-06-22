# AGENTS.md

This folder contains the ASP.NET Core backend for the diploma project.

Project topic:

> “Axborot resurs markazidagi kitoblarning onlayn reytingini shakllantiruvchi axborot tizimini ishlab chiqish”

The backend must provide a clean REST API for managing books, searching books, viewing book details, submitting ratings, and calculating book rating statistics.

## Main stack

Use this stack unless the user explicitly changes it:

* ASP.NET Core Web API
* C#
* PostgreSQL
* Entity Framework Core
* Npgsql EF Core provider
* Meilisearch integration later through backend abstraction
* REST API
* Swagger/OpenAPI for development

Prefer a simple, clear, diploma-friendly architecture over enterprise overengineering.

## Current implementation phase

Focus on backend skeleton and core API.

Do not implement Meilisearch yet unless explicitly asked.

Do not implement authentication yet unless explicitly asked.

Do not implement frontend code from this folder.

The backend should be ready for the existing React Vite frontend to consume later.

## Backend goals

Implement the backend for a book rating information system.

Core features:

* store books;
* list books;
* get book details;
* search books through database initially;
* submit book rating;
* calculate average rating;
* count ratings;
* expose clean API endpoints for frontend integration.

The system is primarily about online book rating for an information resource center.

Do not add borrowing, reservations, library inventory, user roles, payment, notifications, or complex admin workflows unless explicitly requested.

## Preferred solution structure

Create a clear ASP.NET Core solution.

Preferred structure:

```text id="fsswq8"
backend/
  BookRatingSystem.sln
  src/
    BookRatingSystem.Api/
    BookRatingSystem.Application/
    BookRatingSystem.Domain/
    BookRatingSystem.Infrastructure/
  tests/
    BookRatingSystem.Tests/
```

If a simpler structure is chosen for speed, it must still keep API, domain, data access, and business logic reasonably separated.

Do not create unnecessary microservices.

This is a diploma project, not a production distributed system.

## Project responsibilities

### BookRatingSystem.Api

Contains:

* controllers or endpoints;
* request/response DTOs;
* dependency injection setup;
* middleware;
* Swagger/OpenAPI;
* CORS configuration;
* API validation behavior.

### BookRatingSystem.Application

Contains:

* use cases;
* service interfaces;
* application services;
* validation logic;
* DTO mapping logic if needed.

### BookRatingSystem.Domain

Contains:

* core entities;
* domain rules;
* enums;
* value objects if useful.

Keep domain simple and understandable.

### BookRatingSystem.Infrastructure

Contains:

* EF Core DbContext;
* entity configurations;
* repositories if used;
* PostgreSQL implementation;
* search abstraction implementation later;
* database migrations.

## Domain model

Use this initial domain model unless the user changes it.

### Book

Represents a book in the information resource center.

Suggested fields:

```text id="duwvuz"
Id
Title
Author
Category
Description
PublishedYear
CoverImageUrl
CreatedAt
UpdatedAt
```

### BookRating

Represents a rating submitted for a book.

Suggested fields:

```text id="j8ueky"
Id
BookId
Value
Comment
CreatedAt
```

Rating value must be from 1 to 5.

A book can have many ratings.

Average rating and ratings count may be calculated from ratings instead of stored directly unless there is a clear reason to store them.

## Database rules

Use PostgreSQL.

Use EF Core migrations.

Use explicit entity configurations.

Prefer:

* `Guid` or `long` IDs consistently;
* `DateTimeOffset` or UTC `DateTime` consistently;
* required fields for important data;
* reasonable max lengths for text fields;
* indexes for searchable fields.

Add indexes for:

* book title;
* author;
* category;
* rating book id.

Do not hardcode production credentials.

Use configuration from `appsettings.json`, user secrets, or environment variables.

## API endpoints

Implement these initial endpoints:

```text id="z9xzh2"
GET    /api/books
GET    /api/books/{id}
GET    /api/books/search?q=...
POST   /api/books/{id}/ratings
```

Optional, only if needed for demo/admin:

```text id="g434g1"
POST   /api/books
PUT    /api/books/{id}
DELETE /api/books/{id}
```

Do not add endpoints that are not useful for the diploma demo.

## API response expectations

Book list items should include:

```text id="u1m8bt"
id
title
author
category
coverImageUrl
averageRating
ratingsCount
```

Book details should include:

```text id="uk8gnu"
id
title
author
category
description
publishedYear
coverImageUrl
averageRating
ratingsCount
recentRatings
```

Rating submission request:

```json id="s2jlkd"
{
  "value": 5,
  "comment": "Juda foydali kitob"
}
```

Validate rating value from 1 to 5.

Reject invalid requests with clear validation errors.

## Search implementation plan

Initial implementation:

* use PostgreSQL search with simple `ILIKE` or EF Core-compatible filtering;
* search by title, author, and category;
* keep search logic behind an abstraction.

Later implementation:

* integrate Meilisearch through backend;
* index books;
* update index when books are created or updated;
* keep Meilisearch credentials only in backend configuration;
* never expose admin/master keys to frontend.

Create a search abstraction early, for example:

```csharp id="u8d9bb"
public interface IBookSearchService
{
    Task<IReadOnlyList<BookSearchResultDto>> SearchAsync(string query, CancellationToken cancellationToken);
}
```

For the first backend version, implement this using PostgreSQL.

Do not install or configure Meilisearch until explicitly requested.

## CORS

Configure CORS for local frontend development.

Expected frontend origin:

```text id="ow8n21"
http://localhost:5173
```

Keep CORS configuration explicit.

Do not allow all origins in a final/demo configuration unless the user explicitly accepts it.

## Swagger/OpenAPI

Enable Swagger in development.

The API should be easy to test from Swagger UI.

Use meaningful endpoint names, request DTOs, and response DTOs.

## Data seeding

Add simple development seed data if useful.

Seed data should include several books with different authors, categories, and ratings.

Keep seed data realistic but not excessive.

Do not seed copyrighted book descriptions or long copied texts.

## Error handling

Add basic error handling.

The API should not return raw exception details in normal responses.

Use clear errors for:

* book not found;
* invalid rating value;
* empty search query if handled as invalid;
* database errors.

## Validation

Validate incoming requests.

At minimum:

* book title is required;
* author is required;
* rating value must be between 1 and 5;
* comment length should be limited.

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
* complex CQRS/MediatR setup for the first version;
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

The backend must be easy to connect to the existing React Vite frontend.

Before finalizing endpoints, keep response shapes close to the frontend types.

Do not break frontend expectations without reporting it.

If frontend integration requires changes, list them clearly.

## Testing

Add basic tests only if the user asks or if the setup remains simple.

At minimum, keep the project buildable.

Expected commands should work:

```bash id="dqystx"
dotnet restore
dotnet build
dotnet run --project src/BookRatingSystem.Api
```

If migrations are created, document the migration command.

Example:

```bash id="i0i2h7"
dotnet ef database update --project src/BookRatingSystem.Infrastructure --startup-project src/BookRatingSystem.Api
```

## Docker

Do not create Docker setup unless explicitly requested.

If Docker is requested later, keep it simple:

* backend container;
* PostgreSQL container;
* optional Meilisearch container later.

## Secrets and configuration

Do not commit real secrets.

Use placeholders in example configuration.

Allowed placeholder example:

```json id="pxc8pp"
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=book_rating_db;Username=postgres;Password=postgres"
  }
}
```

Make it clear that real credentials should be changed outside source control.

## Diploma compatibility

The backend implementation must support writing the thesis practical chapter.

When adding features, keep them explainable using this structure:

1. Purpose of the module.
2. Used technologies.
3. Data model.
4. API flow.
5. Result achieved.

Prefer simple, demonstrable features over unfinished advanced features.

## Work behavior

Before changing files, inspect the existing project.

Make small, focused, reviewable changes.

Do not delete existing files unless clearly necessary.

Do not rewrite working frontend code from this backend task.

After completing work, respond briefly:

```text id="rdcwwr"
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

Do not implement authentication, admin panel, borrowing, reservation, or inventory modules unless explicitly requested.

Keep the project aligned with the diploma topic: online book rating.
