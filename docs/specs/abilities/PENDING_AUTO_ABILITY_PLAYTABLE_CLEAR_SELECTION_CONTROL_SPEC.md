# Pending Auto Ability PlayTable Clear Selection Control Spec

## Status

Implemented in `M10-61`.

## Purpose

Add an online-only PlayTable control that clears the selected pending AUTO
ability without resolving or mutating gameplay.

## Behavior

- Online PlayTable creates a `ClrAuto` toolbar button.
- Local PlayTable does not create `ClrAuto`.
- `ClrAuto` is disabled until a pending AUTO ability is selected.
- Clicking `ClrAuto` resets selection status to `Pending selection: none`.
- Clearing does not send transport payloads or mutate `GameState`.

## Boundary

This milestone must not:

- resolve pending abilities
- pay costs
- mutate `GameState`
- send clear-selection over transport
- clear trigger draft selection

## Acceptance Tests

- local PlayTable omits `ClrAuto`
- online no-selection state disables `ClrAuto`
- after cycling selection, `ClrAuto` becomes enabled
- clicking `ClrAuto` resets selection status to no selection
- hidden source identity is not surfaced after clear
- clearing does not send pending queue payloads or mutate `GameState`

## Future Extensions

- selected pending ability resolution command
- owner-private choice routing
- prompt row focus/highlight
