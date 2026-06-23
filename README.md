# BookRatingSystem backend

ASP.NET Core Web API for the diploma project book rating system.

## Local services

Backend PostgreSQL and Meilisearch services must be available locally. If they are already running, Docker is not required. The default development configuration expects:

```text
PostgreSQL:  localhost:5432
Meilisearch: http://localhost:7700
```

Required local variables or equivalent `appsettings.json` values:

```text
POSTGRES_DB=book_rating_db
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_PORT=5432
MEILI_MASTER_KEY=dev-master-key-change-me-123456
MEILISEARCH_URL=http://localhost:7700
MEILISEARCH_KEY=dev-master-key-change-me-123456
```

`MEILISEARCH_URL` and `MEILISEARCH_KEY` are used only by the backend. Do not use the development key in production and do not put it in frontend or Vite environment files.

If you need project-local containers, create a local `.env` from `.env.example` and run `docker compose up -d`. Skip this step when PostgreSQL and Meilisearch are already running.

## Run backend

```powershell
dotnet restore
dotnet build
dotnet run --project src/BookRatingSystem.Api
```

Swagger UI is available in Development:

```text
http://localhost:5099/swagger
```

## Database and search index

PostgreSQL is the source of truth. Meilisearch is only a search index.

Apply EF Core migrations when starting from an empty database:

```powershell
dotnet ef database update --project src/BookRatingSystem.Infrastructure/BookRatingSystem.Infrastructure.csproj --startup-project src/BookRatingSystem.Api/BookRatingSystem.Api.csproj
```

In Development, backend startup ensures the `books` Meilisearch index exists, applies index settings, and indexes all books currently stored in PostgreSQL. To rebuild the index, restart the backend after PostgreSQL is available. The startup reindex operation is idempotent and does not clear the index.

Search endpoint:

```text
GET /api/books/search?q=...
```

Public book list, details, and search return only books with `status = Verified`. Books with `New` or `Banned` status remain visible in the admin book table.

The endpoint uses Meilisearch through `IBookSearchService`. If Meilisearch is unavailable and `Meilisearch:UsePostgresFallback` is `true`, backend logs a warning and uses PostgreSQL search as fallback. To surface Meilisearch failures during development, set:

```json
{
  "Meilisearch": {
    "UsePostgresFallback": false
  }
}
```

## Index synchronization

Current implemented flow:

* book creation saves the new book to PostgreSQL, then calls `IBookIndexingService.IndexBookAsync`;
* book update saves changed PostgreSQL data, then calls `IBookIndexingService.IndexBookAsync`; the Meilisearch document is rebuilt from PostgreSQL, including recalculated `averageRating`, `ratingsCount`, and `status`;
* book deletion removes the book from PostgreSQL, then calls `IBookIndexingService.DeleteBookAsync` to remove the document from the search index;
* rating submission saves the rating to PostgreSQL, then re-indexes the rated book so `averageRating` and `ratingsCount` stay fresh in search results.

If Meilisearch synchronization fails after a successful PostgreSQL save, the backend logs a warning and keeps PostgreSQL as the source of truth.

## Authentication and admin endpoints

Seed admin login:

```text
admin / Admin123!
```

Implemented auth/admin endpoints:

```text
POST /api/auth/register
POST /api/auth/login
GET  /api/admin/books
GET  /api/admin/users
GET  /api/admin/ratings
GET  /api/admin/dashboard?from=YYYY-MM-DD&to=YYYY-MM-DD
POST /api/books
PUT  /api/books/{id}
DELETE /api/books/{id}
```

Admin book status values:

```text
New
Banned
Verified
```

Only `Verified` books are shown on the public site.

## Troubleshooting

* If search returns PostgreSQL fallback results, check backend logs for Meilisearch warnings.
* If Meilisearch rejects requests, confirm `MEILI_MASTER_KEY` in Docker matches backend `MEILISEARCH_KEY`.
* If seed books are not indexed, confirm PostgreSQL migrations were applied and restart the backend.
* Do not put Meilisearch keys in frontend or Vite environment files. The browser must call only the ASP.NET Core API.
