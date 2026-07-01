# Reject No-Mutation Guarantee Spec

## Status

Implemented in `M11-05`.

## Purpose

Invalid or unsupported gameplay commands must reject before they mutate live
state. This protects RulesCore, manual pending AUTO queues, multiplayer session
payload histories, replay, and later bot/simulation code from state corruption.

## Boundary

The guarantee applies to reject paths for:

- `RulesCore.TryExecute`
- pending AUTO queue commit helper rejects
- session publish rejects that have no payload to publish

Rejected paths must not:

- change `GameState`
- append to `GameState.event_log`
- normalize null queue lists as a side effect
- change pending AUTO source queues
- append payload histories
- publish network payloads

Accepted commands may mutate state only through the existing legal command/event
path.

## Snapshot Helper

`NoMutationSnapshot` captures state without calling `GameState.ToJson()` or
`PendingAutoAbilityQueue.ToJson()` because those methods normalize lists.
Instead, it uses `JsonUtility.ToJson` directly and records key counts:

- full `GameState` JSON
- `GameState.event_log` count
- committed pending AUTO count
- pending AUTO queue JSON/count
- payload-history collection counts

This helper is a verification guard. It does not enforce runtime behavior by
itself.

## Verified Cases

EditMode coverage verifies:

- null RulesCore action rejects without changing state or event log
- illegal RulesCore action rejects without changing zones, event log, or pending
  AUTO queue
- accepted draw changes the snapshot, proving the guard detects real mutation
- pending AUTO commit mismatch rejects without mutating source queue
- pending AUTO commit with a null source pending list rejects without
  normalizing that list
- missing-payload session publish rejects without changing payload histories,
  sending network payloads, or mutating `GameState`

## Next Work

`M11-06` expands event-sourcing coverage so every accepted mutation has a
replayable `GameEvent`.
