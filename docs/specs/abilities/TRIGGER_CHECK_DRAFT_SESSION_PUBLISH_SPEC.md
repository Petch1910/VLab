# Trigger Check Draft Session Publish Spec

## Status

Implemented in `M10-18`.

## Purpose

Let the multiplayer game-session controller publish a manual trigger-check draft
payload without requiring UI callers to supply room id or sender id directly.

The helper fills session context, calls the manual draft payload builder, sends
through the existing trigger-check replay transport hook, and stores the sent
payload in the local session payload list.

## Inputs

- `MultiplayerGameSessionController.State`
- current `MultiplayerRoomState`
- local player id
- explicit `ManualTriggerCheckDraftRequest` card/check/trigger fields

## Output

- `ManualTriggerCheckDraftResult`
- sent `NetworkTriggerCheckReplayLogPayload`
- local session `TriggerCheckReplayLogPayloads` entry on successful send

## Boundary

The session helper must not:

- trust caller-supplied room id or sender id
- send invalid draft payloads
- inspect hidden deck order
- move cards
- apply trigger modifiers
- append to `GameState.event_log`

## Acceptance Tests

- session fills room id and sender id from its own context
- valid draft payload is sent and stored locally
- caller request is not mutated
- invalid draft input is rejected without sending
- repeated identical input sends deterministic payload JSON
- `GameState` is unchanged after publish attempts
- Unity compile and EditMode tests pass

## Future Extensions

- PlayTable manual draft publish control
- trigger type picker for manual trigger draft creation
- RulesCore trigger-check action once reveal/move/apply behavior is implemented
