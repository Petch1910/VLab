# Pending Auto Ability PlayTable Selection Cycle Control Spec

## Status

Implemented in `M10-60`.

## Purpose

Add an online-only PlayTable control that cycles a read-only selected pending
AUTO ability from the latest decoded queue.

This makes the future prompt workflow visible without resolving abilities or
mutating gameplay state.

## Behavior

- Online PlayTable creates a `SelAuto` toolbar button.
- Local PlayTable does not create `SelAuto`.
- `SelAuto` is disabled when no pending AUTO queue payload is stored.
- Clicking `SelAuto` decodes the latest payload and selects the next pending
  item by index.
- Selection status is rendered through
  `PendingAutoAbilitySelectionStatusFormatter`.
- Hidden source identities stay hidden.
- Cycling selection does not send transport payloads or mutate `GameState`.

## Boundary

This milestone must not:

- resolve pending abilities
- pay costs
- mutate `GameState`
- send selection over transport
- create owner-private choice routing

## Acceptance Tests

- local PlayTable omits `SelAuto`
- online no-payload state disables `SelAuto`
- online payload selection updates selection status
- repeated clicks cycle pending items
- hidden source identity remains hidden
- cycling does not send pending queue payloads or mutate `GameState`

## Future Extensions

- clear selected pending ability control
- resolve selected pending ability command factory
- owner-private selection routing
