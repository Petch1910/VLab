# Pending Auto Ability PlayTable Publish Control Spec

## Status

Implemented in `M10-54`.

## Purpose

Expose a small online-only PlayTable control that republishes the latest stored
pending AUTO ability queue payload through the existing session/transport seam.

This is a diagnostic scaffold. It does not resolve abilities, ask players to
choose ability order, pay costs, or apply queue mutations to `GameState`.

## Behavior

- Online PlayTable creates an `AutoQ` toolbar button.
- Local PlayTable does not create the `AutoQ` button.
- `AutoQ` is disabled when the session has no stored pending AUTO queue payload.
- When a payload exists, clicking `AutoQ` calls
  `MultiplayerGameSessionController.PublishLatestPendingAutoAbilityQueue`.
- Successful publish shows a deterministic status message.
- Publishing does not mutate `GameState`.

## Boundary

This milestone must not:

- resolve pending AUTO abilities
- create ability-order prompt UI
- alter the pending queue model
- add card-specific ability execution
- route owner-private decisions

## Acceptance Tests

- online no-payload table has disabled `AutoQ`
- online table with a stored payload publishes through transport
- local table omits `AutoQ`
- publishing preserves `GameState`

## Future Extensions

- pending ability prompt/ordering UI
- legal ability resolution commands
- owner-private ability-choice routing
- public/spectator masking for ability prompt choices
