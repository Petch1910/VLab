# Trigger Check Draft Request Factory Spec

## Status

Implemented in `M10-35`.

## Purpose

Extract PlayTable construction of `ManualTriggerCheckDraftRequest` into a pure
UI factory.

This keeps manual trigger-check draft publishing deterministic while preventing
the PlayTable UI from owning protocol-object defaults directly.

## Inputs

- player index
- trigger check source
- trigger check index
- selected card instance id
- selected card id
- draft trigger type

## Output

- a `ManualTriggerCheckDraftRequest` with copied explicit fields
- fixed manual draft defaults:
  - `modifier_expiration = EndOfTurn`
  - `perspective = Spectator`
  - `viewer_player_index = -1`

## Boundary

The factory must not:

- know about Unity UI objects
- validate selected-card visibility
- send payloads
- move cards
- inspect deck order
- apply trigger bonuses to `GameState`
- append to `GameState.event_log`
- mutate `GameState`

Validation and session fields such as room id and sender player id remain owned
by `TriggerCheckDraftSelectionValidator` and `MultiplayerGameSessionController`.

## Acceptance Tests

- factory copies player/check/card/trigger fields
- factory applies fixed spectator/end-of-turn defaults
- nullable selected-card ids are passed through for caller validation
- PlayTable uses the factory without changing payload fields
- Unity compile and EditMode tests pass

## Future Extensions

- configurable view perspective for owner-visible draft previews
- factory overload for resolved selected-card context objects
- shared request construction for compact mobile trigger-check controls
