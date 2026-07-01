# PlayTable Action Status Formatter Spec

## Status

Implemented in `M10-41`.

## Purpose

Extract PlayTable action fallback/status messages into a pure UI formatter.

This keeps action feedback deterministic and covered by tests while preserving
the current draw, phase, gift marker, and online undo-disabled text.

## Inputs

- target game phase
- target gift marker type

## Output

- draw fallback: `Cannot draw.`
- phase fallback: `Cannot set phase to <phase>.`
- gift marker fallback: `Cannot add <marker_type> marker.`
- online undo status: `Undo is disabled in online rooms.`

## Boundary

The formatter must not:

- inspect Unity UI objects
- execute legal actions
- move cards
- inspect deck order
- send multiplayer payloads
- append to `GameState.event_log`
- mutate `GameState`

## Acceptance Tests

- draw fallback message matches existing text
- phase fallback message preserves phase text
- gift marker fallback message preserves marker text
- online undo-disabled message matches existing text
- PlayTable uses the formatter without changing action status text
- Unity compile and EditMode tests pass

## Future Extensions

- localized action messages
- compact mobile action feedback
- status severity metadata for toast/log rendering
