# PlayTable Card Selection Status Formatter Spec

## Status

Implemented in `M10-40`.

## Purpose

Extract PlayTable card selection status messages into a pure UI formatter.

This keeps selection feedback deterministic and reusable for future compact or
mobile controls while preserving the current user-facing text.

## Inputs

- selected card id
- selected card zone

## Output

- select-first message: `Select a card first.`
- selected card message: `Selected <card_id> from <zone>.`
- no-card-selected message: `No card selected.`

## Boundary

The formatter must not:

- inspect Unity UI objects
- move cards
- validate legal actions
- inspect deck order
- send multiplayer payloads
- append to `GameState.event_log`
- mutate `GameState`

## Acceptance Tests

- select-first message matches existing text
- selected-card message preserves card id and zone text
- no-card-selected message matches existing text
- null card ids preserve prior string-concatenation behavior
- PlayTable uses the formatter without changing selection text
- Unity compile and EditMode tests pass

## Future Extensions

- localized selection messages
- compact mobile selection text
- richer selected-card context for image/detail panes
