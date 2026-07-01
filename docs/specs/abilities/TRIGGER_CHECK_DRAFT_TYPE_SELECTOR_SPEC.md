# Trigger Check Draft Type Selector Spec

## Status

Implemented in `M10-20`.

## Purpose

Let the online PlayTable choose the trigger type used by manual trigger-check
draft publishing before pressing `DraftTrig`.

The selector changes only local UI state. It does not resolve trigger effects,
apply modifiers, or alter the game state by itself.

## Inputs

- online PlayTable `TrigType` button
- current local draft trigger type

## Output

- default draft trigger type is `Unknown`
- `TrigType` cycles through `Critical`, `Draw`, `Front`, `Heal`, `Over`,
  `None`, and back to `Unknown`
- `DraftTrig` uses the selected trigger type when building the draft payload

## Boundary

The selector must not:

- appear in local mode
- inspect cards or deck order
- send payloads by itself
- move cards
- apply trigger bonuses to `GameState`
- append to `GameState.event_log`

## Acceptance Tests

- online table shows the default `Unknown` selector state
- cycling once changes the selector to `Critical`
- publishing after cycling sends a `Critical` draft payload
- publishing still requires selected-card context
- local table does not create `TrigType`
- Unity compile and EditMode tests pass

## Future Extensions

- check source selector (`Manual`, `Drive`, `Damage`)
- check index control for twin-drive/damage sequences
- visual badge showing selected draft type in the trigger summary panel
