# Average-rating ordering design

## Goal

Order public book results by average rating without changing the existing route contracts, filtering, search matching, pagination, DTOs, or error handling.

## Scope

Apply the same deterministic ordering before pagination in every data path used by:

- `GET /api/books`
- `GET /api/books/search` through Meilisearch
- `GET /api/books/search` through the PostgreSQL fallback

The order is:

1. Average of verified ratings, descending.
2. Number of verified ratings, descending.
3. Book title, ascending.

Books without verified ratings have an average of zero and therefore follow positively rated books. Category and verification-status filters remain unchanged.

## Implementation boundaries

- Change only the ordering clauses in the existing repository and search services.
- Reuse the existing indexed `averageRating` and `ratingsCount` fields in Meilisearch.
- Do not add query parameters, endpoints, database columns, migrations, DTO fields, or new business rules.
- Preserve total-count calculations and apply pagination after ordering.

## Verification

Add focused regression tests proving that public book fetches and search fallback results are ordered by verified average rating with stable tie-breakers. Verify the Meilisearch request specifies the equivalent sort order, then run the complete test suite.
