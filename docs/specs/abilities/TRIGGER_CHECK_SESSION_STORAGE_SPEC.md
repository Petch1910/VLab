# Trigger Check Session Storage Spec

## Status

Implemented in `M10-14`.

## Purpose

Store received `NetworkTriggerCheckReplayLogPayload` records in
`MultiplayerGameSessionController` so future UI/replay panels can display
masked trigger check diagnostics.

Storage must stay separate from `GameState.event_log` because trigger check
payloads are advisory/debug replay data, not live gameplay mutations.

## Inputs

- `IMultiplayerTransport.TriggerCheckReplayLogReceived`
- `NetworkTriggerCheckReplayLogPayload`

## Output

- read-only received trigger check replay payload list
- state changed notification for UI refresh
- last message update for debugging

## Boundary

The storage hook must not:

- mutate `GameState`
- append to `GameState.event_log`
- apply combat modifiers
- send trigger logs automatically
- block normal game-event sync

## Acceptance Tests

- received trigger check replay payload is stored
- receiving a trigger check replay payload does not mutate `GameState`
- normal game-event sync still works after trigger-log receipt
- null payload is ignored
- Unity compile and EditMode tests pass

## Future Extensions

- PlayTable panel for trigger check log summaries
- replay panel merge of gameplay events and trigger check diagnostics
- live Photon smoke test for trigger check payload delivery
