# Ability Trigger GameState Adapter Spec

## Status

Implemented in `M10-46`; extended in `M10-110`.

## Purpose

Compose `GameState.pending_auto_abilities` with
`AbilityTriggerEventCollector`.

The adapter keeps a read-only collection method for inspection and adds an
explicit commit method for `M10-110` so real `GameEvent` timing events can add
pending AUTO entries to `GameState.pending_auto_abilities` without UI, bot, or
network code mutating state directly.

## Read-Only Behavior

- Reads the current pending auto ability queue from `GameState`.
- Passes that queue, one `GameEvent`, and explicit trigger registrations to the
  collector.
- Returns a new `PendingAutoAbilityQueue`.
- Handles null `GameState` as an empty/default pending queue.

## Commit Behavior

`CommitPendingQueueFromTimingEvent`:

- requires a non-null `GameState` and non-null `GameEvent`
- uses the read-only collection path to produce a queue clone
- assigns the cloned queue to `GameState.pending_auto_abilities` only when the
  real timing event adds at least one pending AUTO entry
- leaves the source queue reference unchanged when no registration matches
- reports before/after counts, added count, timing event, source event id, and
  whether state was updated

This commit path is the intended mutation boundary for pending AUTO entries
created from real game/action timing events.

## Boundary

The adapter must not:

- call `GameState.EnsureLists`
- append directly into an existing `GameState.pending_auto_abilities.pending`
  list
- resolve ability text or effects
- pay costs
- publish multiplayer payloads
- mutate `GameState.event_log`
- mutate UI/session/network state

## Acceptance Tests

- matching registrations append to a copied queue read from `GameState`
- null state returns a default queue with collected items when inputs match
- source `GameState` queue remains unchanged
- null queue/list data on the source state remains unchanged
- committing from a `RulesCore`-produced `GameEvent` updates
  `GameState.pending_auto_abilities`
- unmatched timing events leave `GameState.pending_auto_abilities` unchanged
- committed collection does not append to `GameState.event_log`
- missing state/event rejects without mutation

## Future Extensions

- owner/private masking for pending ability prompts
- simultaneous AUTO priority choice UI
- replay-log entry for ability queue commits
