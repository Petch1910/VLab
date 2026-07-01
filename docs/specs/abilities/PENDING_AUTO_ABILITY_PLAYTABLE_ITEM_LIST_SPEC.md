# Pending Auto Ability PlayTable Item List Spec

## Status

Implemented in `M10-56`.

## Purpose

Show the decoded pending AUTO ability item list on the PlayTable as a
read-only diagnostic surface.

This prepares the UI for a future ability-order prompt without allowing the UI
to resolve abilities or mutate gameplay state.

## Behavior

- PlayTable creates a `Pending Ability Items` text surface.
- Local PlayTable shows deterministic zero pending item output.
- Online PlayTable formats the latest stored pending AUTO ability queue payload
  through `PendingAutoAbilityItemListFormatter`.
- The surface caps visible items to keep the side panel compact.
- Hidden source identities remain hidden.

## Boundary

This milestone must not:

- add selection controls for pending abilities
- resolve abilities
- pay costs
- mutate `GameState`
- send transport payloads while rendering

## Acceptance Tests

- local PlayTable reports zero pending items
- online PlayTable shows decoded pending item metadata
- hidden source ids stay hidden
- rendering does not mutate `GameState`
- rendering does not send pending queue payloads

## Future Extensions

- pending ability row selection
- resolve/cancel prompt controls
- owner-private choice routing
