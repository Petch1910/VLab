# ADR-0002: Use SQLite For Local Structured Data

## Status

Accepted

## Context

The app needs offline card search, deck storage, replay storage, settings, and card pack metadata. The current card pool has `10,836` cards and image files around `2.163 GiB`.

## Decision

Use SQLite for structured local data. Store images as files and keep only image metadata/path in SQLite.

## Consequences

Positive:

- Offline friendly
- Simple distribution
- Good enough for card/deck/replay queries

Tradeoffs:

- Need migration/version strategy
- Need separate image cache management
- Full text search may require SQLite FTS or a custom search index

