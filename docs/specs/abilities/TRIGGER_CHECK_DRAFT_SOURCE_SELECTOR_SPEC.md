# Trigger Check Draft Source Selector Spec

## Status

Implemented in `M10-21`.

## Purpose

Let the online PlayTable choose the check source metadata used by manual
trigger-check draft publishing before pressing `DraftTrig`.

The selector is metadata-only. It does not infer the source from the deck,
perform a drive check, perform a damage check, move cards, or apply trigger
effects.

## Inputs

- online PlayTable `ChkSrc` button
- current local draft check source

## Output

- default draft check source is `Manual`
- `ChkSrc` cycles through `Drive`, `Damage`, and back to `Manual`
- `DraftTrig` uses the selected check source when building the draft payload

## Boundary

The selector must not:

- appear in local mode
- inspect deck order
- send payloads by itself
- move cards
- apply trigger bonuses to `GameState`
- append to `GameState.event_log`

## Acceptance Tests

- online table shows default `Manual` source state
- cycling once changes the selector to `Drive`
- publishing after cycling sends a `Drive` source draft payload
- publishing still requires selected-card context
- local table does not create `ChkSrc`
- Unity compile and EditMode tests pass

## Future Extensions

- check index/sequence selector for multi-check flows
- side-panel compact summary of draft type/source/index
- real RulesCore trigger-check command after reveal/move/apply rules are owned
  by the core
