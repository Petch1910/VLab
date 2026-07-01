# Trigger Check Manual Draft Payload Spec

## Status

Implemented in `M10-17`.

## Purpose

Create a deterministic trigger-check replay payload from explicit manual input
without reading hidden deck order or mutating the live game state.

This bridges manual trigger-check entry and the existing replay-log/network
payload pipeline. It is still advisory: it does not resolve a real game action.

## Inputs

- current `GameState` for visible target evaluation only
- room id and sender player id
- explicit checked-card instance id and card id
- explicit `TriggerCheckSource`, check index, trigger type, expiration, and
  output perspective

## Output

- `ManualTriggerCheckDraftResult`
- masked `NetworkTriggerCheckReplayLogPayload` using
  `TriggerCheckReplayLogPayloadCodec`
- deterministic payload JSON for identical input

## Boundary

The draft builder must not:

- inspect deck order
- infer the checked card from hidden state
- move cards
- apply trigger modifiers to `GameState`
- append to `GameState.event_log`
- send network payloads directly

## Acceptance Tests

- spectator output masks checked-card identity
- owner player output can reveal that player's checked card
- output is deterministic for identical input
- missing required input is rejected
- unknown trigger type creates a manual-resolution payload
- `GameState` is unchanged after draft creation
- Unity compile and EditMode tests pass

## Future Extensions

- session-level helper that fills room/sender values and publishes a draft
- PlayTable manual input controls for explicit trigger check drafting
- real RulesCore trigger-check command after deterministic reveal/state movement
  is owned by the core
