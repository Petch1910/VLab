# Trigger Check Log Entry Spec

## Status

Implemented in `M10-08`.

## Purpose

Create a compact replay/debug log entry from a `TriggerCheckResolutionBundle`
without extending the live `GameEvent` reducer yet.

This lets future UI/replay/bot diagnostics display what a trigger check
resolved into while the actual card movement and event-log integration remain
future work.

## Inputs

- `TriggerCheckResolutionBundle`

## Output

- deterministic log entry id
- check source/player/index metadata
- checked card instance id and card id
- trigger type
- accepted/manual status
- rejection reason
- generated combat modifier ids
- notes
- summary

## Boundary

The helper must not:

- mutate `GameState`
- add to `GameState.event_log`
- require a new `GameActionType`
- send Photon payloads
- replay or apply trigger effects

## Acceptance Tests

- accepted critical bundle creates a log entry with modifier ids
- no-trigger bundle creates a log entry without modifiers
- log entry JSON round-trips
- repeated log entry creation is deterministic
- input `GameState` remains unchanged when deriving a bundle and log entry

## Future Extensions

- live `GameEvent` payload extension or parallel effect log stream
- public/private event masking for trigger checks
- replay UI display of trigger check bundles
- Photon payload codec for trigger check advisory logs
