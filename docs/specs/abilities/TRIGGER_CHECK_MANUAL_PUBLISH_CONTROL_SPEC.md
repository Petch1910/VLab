# Trigger Check Manual Publish Control Spec

## Status

Implemented in `M10-16`.

## Purpose

Provide a small online PlayTable control that republishes the latest stored
trigger check replay-log payload through the multiplayer transport.

This is an advisory log-sharing control only. It exists so future manual trigger
check flows can share masked trigger-check diagnostics without changing the live
game state.

## Inputs

- `MultiplayerGameSessionController.TriggerCheckReplayLogPayloads`
- latest already-masked `NetworkTriggerCheckReplayLogPayload`
- online `IMultiplayerTransport.SendTriggerCheckReplayLog`

## Output

- online PlayTable creates a `TrigLog` button
- the button is disabled until a stored trigger-check replay payload exists
- pressing the button sends the latest stored payload through the session
  controller
- local PlayTable does not create the publish button

## Boundary

The manual publish control must not:

- inspect or reveal the deck
- create new trigger check results
- resolve trigger effects
- apply combat modifiers
- move cards between zones
- append to `GameState.event_log`
- mutate `GameState`

## Acceptance Tests

- online PlayTable creates the `TrigLog` control
- online PlayTable disables the control when no trigger payload exists
- online PlayTable publishes the latest stored trigger payload deterministically
- local PlayTable does not create the publish control
- publishing does not mutate `GameState` or append to `GameState.event_log`
- Unity compile and EditMode tests pass

## Future Extensions

- manual trigger-check draft creation from explicit user input
- detail panel for selecting which stored trigger log to republish
- live trigger-check action once RulesCore owns deterministic reveal and card
  movement
