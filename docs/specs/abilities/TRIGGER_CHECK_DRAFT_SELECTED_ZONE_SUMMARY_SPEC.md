# Trigger Check Draft Selected-Zone Summary Spec

## Status

Implemented in `M10-25`.

## Purpose

Extend the read-only PlayTable trigger-check draft summary to show which zone
the selected card came from.

This clarifies what `DraftTrig` will use as manual draft context while keeping
card movement, trigger resolution, and payload publishing as separate actions.

## Inputs

- current selected card id
- current selected card zone
- current draft trigger type
- current draft check source
- current draft check index

## Output

- no selected card: `zone none`
- selected card: `zone <GameZone>`
- summary still includes trigger type, check source, check index, and selected
  card id

## Boundary

The selected-zone summary must not:

- send payloads
- inspect hidden card identity
- inspect deck order
- move cards
- apply trigger bonuses to `GameState`
- append to `GameState.event_log`

## Acceptance Tests

- online summary shows `zone none` before selection
- selecting a visible card updates the summary with that card's zone
- summary updates do not send payloads
- local summary remains local-mode text
- `GameState` is unchanged after selection-only summary updates
- Unity compile and EditMode tests pass

## Future Extensions

- explicit selected-card clear control
- selected-card/zone draft status badge
- separate detail rows for draft metadata when the toolbar grows too dense
