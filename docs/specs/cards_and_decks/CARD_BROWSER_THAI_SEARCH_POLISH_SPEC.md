# Card Browser Thai Search Polish Spec

## Status

Implemented in `M16-07`.

## Purpose

Polish Card Browser search feedback for Thai and English queries without
changing card pack data, SQLite schema, or repository filtering semantics.

## Scope

`M16-07` adds a pure formatter for:

- empty-result detail text
- active query/series/clan labels
- whitespace-normalized display strings

`CardBrowserBootstrap` uses the formatter when the current card query returns no
cards.

## Boundaries

The formatter must not:

- query SQLite
- mutate `CardQueryOptions`
- change `SqliteCardRepository.QueryCards`
- alter card pack JSON/SQLite data
- alter deck contents or validation
- touch `GameState`

## Output Shape

No active filters:

```text
No active search filters.
```

Active filters:

```text
Query: "<thai-or-english-query>"
Series: <series>
Clan: <clan>
Try clearing search or filters.
```

## Acceptance Tests

- Thai query text is preserved in the visible empty-result message
- English query text is preserved in the visible empty-result message
- search whitespace is normalized for display
- blank query/series/clan values are omitted
- no-filter empty result gives a clear default message
- formatting does not mutate `CardQueryOptions`
