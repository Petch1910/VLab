# Deck Builder Filter Polish Spec

## Status

Implemented in `M16-06`.

## Purpose

Polish Deck Builder filter and deck status text without changing card queries,
deck contents, validation semantics, or save/load behavior.

## Scope

`M16-06` adds a pure UI formatter for:

- card pool result count and active filters
- deck count/playable status
- validation issue summary

The formatter is used by `CardBrowserBootstrap` only for read-only text.

## Boundaries

The formatter must not:

- query SQLite
- mutate `VanguardDeck`
- mutate `DeckValidationResult`
- add or remove cards
- change deck validation rules
- change card repository filtering
- touch `GameState`

## Output Shape

Card pool status:

```text
Card pool | Total <total> | Showing <count> | Filters: <none/search/series/clan> | Pack <version> | <pack validation summary>
```

Deck status:

```text
Deck status
Main <count>/50 | Ride <count>/4 | G <count>/16
Issues: <errors> errors / <warnings> warnings
Playable: yes|no
```

Validation issue summary:

```text
No validation issues.
```

or up to a bounded list of error/warning codes plus a remaining issue count.

## Acceptance Tests

- no-filter card pool status is compact and deterministic
- active search/series/clan filters are visible
- blank filter values are normalized away
- deck status includes counts, issue totals, and playable state
- issue summary is bounded and preserves error/warning severity
- formatter calls do not mutate deck validation results
- `CardBrowserBootstrap` uses the formatter for the status and deck panels
