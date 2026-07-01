# Trigger Check Draft Clear-Selection Control Spec

## Status

Implemented in `M10-26`.

## Purpose

Add an online PlayTable control that clears the selected-card context used by
manual trigger-check draft publishing.

This gives users a way to reset draft context without moving cards or affecting
the live board.

## Inputs

- current selected card instance id
- current selected card id
- current selected card zone

## Output

- online-only `ClrDraft` button
- button is disabled when no selected-card context exists
- pressing the button clears selected card context
- `DraftTrig` becomes disabled after clearing
- draft summary returns to `card none / zone none`

## Boundary

The clear-selection control must not:

- appear in local mode
- send payloads
- move cards
- inspect deck order
- apply trigger bonuses to `GameState`
- append to `GameState.event_log`

## Acceptance Tests

- online `ClrDraft` exists and is disabled before selecting a card
- selecting a card enables `ClrDraft` and `DraftTrig`
- pressing `ClrDraft` disables `DraftTrig`
- summary resets to `card none / zone none`
- clearing does not send payloads
- `GameState` is unchanged after clearing
- Unity compile and EditMode tests pass

## Future Extensions

- keyboard shortcut for clearing draft selection
- compact clear icon when the toolbar is redesigned
- separate selected-card status component shared by manual draft and future
  real trigger-check UI
