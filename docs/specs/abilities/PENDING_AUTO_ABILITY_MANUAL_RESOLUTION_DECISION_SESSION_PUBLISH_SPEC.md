# Pending Auto Ability Manual Resolution Decision Session Publish Spec

## Status

Implemented in `M10-79`.

## Purpose

Publish the latest stored pending AUTO ability manual resolution decision
payload through the multiplayer game session controller.

This milestone only republishes an already stored network payload. It does not
create decisions from UI state, add PlayTable controls, or apply decision
effects.

## Behavior

- `CanPublishPendingAutoAbilityManualResolutionDecision` is true when at least
  one decision payload is stored.
- `PublishLatestPendingAutoAbilityManualResolutionDecision` rejects when no
  stored payload exists.
- On success, the latest stored payload is sent through
  `IMultiplayerTransport.SendPendingAutoAbilityManualResolutionDecision`.
- Stored payload history remains available after publishing.
- `GameState` and `GameState.event_log` are not mutated.

## Rejections

- No stored decision payload:
  `PENDING_AUTO_ABILITY_MANUAL_RESOLUTION_DECISION_MISSING`

## Boundary

This milestone must not:

- create a new decision payload from UI or selected ability state
- resolve, skip, or defer pending abilities
- mutate `GameState`
- add PlayTable controls

## Acceptance Tests

- missing-payload publish is rejected
- latest stored payload is sent through transport
- stored payload remains available after publish
- `GameState` is unchanged
