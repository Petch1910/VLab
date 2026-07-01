# Pending Auto Ability Real Timing Event Integration Spec

## Status

Implemented in `M10-110`.

## Purpose

Move pending AUTO queue creation from a manual/scaffold-only concept to an
explicit integration path that consumes real `GameEvent` timing events produced
by the current RulesCore action flow.

This is not full automatic card effect resolution. It only creates pending AUTO
queue entries for registered timing events.

## Flow

1. A gameplay command is accepted by `RulesCore.TryExecute`.
2. RulesCore returns the committed `GameEvent`.
3. `AbilityTriggerGameStateAdapter.CommitPendingQueueFromTimingEvent` receives:
   - the live `GameState`
   - that committed `GameEvent`
   - explicit `AbilityTriggerRegistration` data
4. The adapter calls the pure `AbilityTriggerEventCollector`.
5. If at least one registration matches the real timing event, the adapter
   replaces `GameState.pending_auto_abilities` with the collected queue clone.
6. If nothing matches, the existing `GameState.pending_auto_abilities` reference
   remains unchanged.

## Boundaries

The integration must not:

- publish pending AUTO queue payloads to multiplayer transport
- mutate UI/session payload histories
- append to `GameState.event_log`
- resolve ability effects
- pay costs
- choose targets
- parse live card text
- expose hidden source identity through public payloads

The adapter may mutate only this field through the explicit commit method:

- `GameState.pending_auto_abilities`

## Result Metadata

The commit result records:

- accepted/rejected
- rejection reason
- source event id
- timing event
- before pending count
- after pending count
- added count
- whether state was updated
- collected pending queue clone

## Acceptance Tests

- a `RulesCore`-produced draw event with `OnDraw` registration creates one
  pending AUTO entry in `GameState.pending_auto_abilities`
- unmatched timing events leave the state queue reference unchanged
- matching timing events copy the source queue before state assignment
- committed collection does not append to `GameState.event_log`
- missing state/event rejects without mutation

## Next Work

`M10-111` will add a committed manual-resolved event for unsupported abilities.
Structured ability execution remains blocked until `M12`.
