# Trigger Check Draft Selected-Card Status Helper Spec

## Status

Implemented in `M10-27`.

## Purpose

Extract selected-card draft status text into a small pure UI helper so PlayTable
and future trigger-check UI surfaces can share the same card and zone labels.

This keeps the existing `DraftTrig` behavior unchanged while reducing formatting
duplication before the trigger-check UI grows.

## Inputs

- selected card id
- selected card instance id
- selected card zone

## Output

- selected status text: `card <card-label> / zone <zone-label>`
- no selected card id: `card none`
- selected visible card: `card <short-card-id>`
- hidden card identity: `card hidden`
- no selected card instance id: `zone none`
- selected card instance id: `zone <GameZone>`
- visible card ids longer than 18 characters are shortened to the first 18
  characters

## Boundary

The helper must not:

- send payloads
- move cards
- inspect hidden card identity beyond the public hidden sentinel
- inspect deck order
- apply trigger bonuses to `GameState`
- append to `GameState.event_log`
- mutate `GameState`

## Acceptance Tests

- none selection formats as `card none / zone none`
- visible card and zone labels are deterministic
- hidden card formats as `card hidden`
- missing selected-card instance formats zone as `none`
- long card ids shorten deterministically
- PlayTable summary uses the helper without behavior change
- Unity compile and EditMode tests pass

## Future Extensions

- pure formatter for trigger draft metadata: type, check source, and index
- pure formatter for the full draft summary line
- compact draft status component shared by manual draft and real trigger-check UI
