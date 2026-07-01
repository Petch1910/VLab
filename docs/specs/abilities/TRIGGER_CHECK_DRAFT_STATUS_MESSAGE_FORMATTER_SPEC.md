# Trigger Check Draft Status Message Formatter Spec

## Status

Implemented in `M10-34`.

## Purpose

Extract PlayTable manual trigger-check draft status messages into a pure UI
formatter.

This keeps the current user-facing status text unchanged while making selector
and clear-selection messages reusable for future compact or mobile controls.

## Inputs

- draft trigger type
- draft check source
- draft check index

## Output

- trigger type status: `Draft trigger type: <TriggerType>.`
- check source status: `Draft check source: <TriggerCheckSource>.`
- check index status: `Draft check index: <index>.`
- clear selection status: `Draft selection cleared.`

## Boundary

The formatter must not:

- know about Unity UI objects
- send payloads
- move cards
- inspect card identity
- inspect deck order
- apply trigger bonuses to `GameState`
- append to `GameState.event_log`
- mutate `GameState`

## Acceptance Tests

- trigger type status matches existing text
- check source status matches existing text
- check index status matches existing text
- clear selection status matches existing text
- PlayTable uses the formatter without changing user-facing text
- Unity compile and EditMode tests pass

## Future Extensions

- localized status strings
- disabled-state reason messages
- shared status rendering for manual draft and real trigger-check UI
