# Trigger Check Draft Index Selector Spec

## Status

Implemented in `M10-22`.

## Purpose

Let the online PlayTable choose the check index metadata used by manual
trigger-check draft publishing before pressing `DraftTrig`.

The selector is metadata-only. It does not inspect deck order, perform a real
drive/damage check, move cards, or apply trigger effects.

## Inputs

- online PlayTable `ChkIdx` button
- current local draft check index

## Output

- default draft check index is `0`
- `ChkIdx` cycles through `0`, `1`, `2`, and `3`
- `DraftTrig` uses the selected check index when building the draft payload

## Boundary

The selector must not:

- appear in local mode
- inspect deck order
- infer check count from actual drive/damage mechanics
- send payloads by itself
- move cards
- apply trigger bonuses to `GameState`
- append to `GameState.event_log`

## Acceptance Tests

- online table shows default `Idx 0`
- cycling once changes the selector to `Idx 1`
- publishing after cycling sends a draft payload with check index `1`
- publishing still requires selected-card context
- local table does not create `ChkIdx`
- Unity compile and EditMode tests pass

## Future Extensions

- compact side-panel draft summary showing type/source/index together
- explicit check sequence UI for twin drive, triple drive, and multiple damage
  checks
- real RulesCore trigger-check command after reveal/move/apply rules are owned
  by the core
