# Trigger Check Draft Selected-Card Summary Spec

## Status

Implemented in `M10-24`.

## Purpose

Extend the read-only PlayTable trigger-check draft summary to show which
selected card id will be used by `DraftTrig`.

This makes the manual draft flow less ambiguous while keeping card movement,
trigger resolution, and payload publishing as separate actions.

## Inputs

- current selected card id
- current draft trigger type
- current draft check source
- current draft check index

## Output

- no selected card: `card none`
- selected visible card: `card <short-card-id>`
- hidden card identity: `card hidden`
- summary still includes trigger type, check source, and check index

## Boundary

The selected-card summary must not:

- send payloads
- inspect hidden card identity
- inspect deck order
- move cards
- apply trigger bonuses to `GameState`
- append to `GameState.event_log`

## Acceptance Tests

- online summary shows `card none` before selection
- selecting a visible card updates the summary with that card id
- summary updates do not send payloads
- local summary remains local-mode text
- `GameState` is unchanged after selection-only summary updates
- Unity compile and EditMode tests pass

## Future Extensions

- show selected card zone in the summary
- shorten long card ids consistently with card buttons
- add an explicit selected-card clear control
