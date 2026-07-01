# Pending Auto Ability Queue Commit Policy Spec

Status: Implemented in M10-104.

## Purpose

Define the boundary between preview-only pending AUTO queue changes and
committed pending AUTO queue changes before implementing commit helpers/events.

This policy exists to prevent the PlayTable, bot, or network layer from turning
preview results into gameplay state without a replayable event boundary.

## Terms

### Preview Queue

A preview queue is the cloned queue returned by apply-preview logic such as
`PendingAutoAbilityManualResolutionApplyExecutor.Apply`.

Rules:

- It is a what-if result.
- It may be stored in session payload history for UI/debug display.
- It must not be treated as authoritative gameplay state.
- It must not write to `GameState.event_log`.
- It must not be replayed as a committed game action.

### Committed Queue

A committed queue is the authoritative pending AUTO queue after a legal
resolution decision has been accepted by the core command path.

Rules:

- It must be produced through a pure commit helper first.
- It must later be represented by a replayable `GameEvent`.
- It must be reproducible from event replay.
- UI, bot, and network code must not mutate it directly.

### Manual Unsupported Resolution

`Resolve` remains a manual unsupported resolution marker until structured
ability execution exists.

Rules:

- `Resolve` may mark the pending AUTO item as manually handled only after the
  committed helper/event path exists.
- `Resolve` must not execute card text automatically in M10.
- `Skip` and `Defer` may change queue order/content after validation.

## Current M10 Behavior

Through M10-103:

- `ApplyDec` is preview-only.
- Accepted apply preview may append a preview queue payload to session storage.
- Accepted/rejected apply preview appends a safe session preview log entry.
- No preview path publishes network payloads.
- No preview path mutates `GameState`.
- No preview path writes to `GameState.event_log`.

## M10-105 Helper Contract

The next helper must be pure and must not mutate input queue objects.

Allowed inputs:

- current committed queue state,
- validated manual resolution decision,
- selected commit policy target.

Required outputs:

- accepted/rejected result,
- next committed queue clone when accepted,
- stable reason when rejected,
- enough metadata for M10-106 to build a replayable event.

Required validation:

- queue exists and has pending entries,
- decision exists,
- decision type is supported,
- decision pending id matches the active pending entry,
- selected queue state target is explicit.

Allowed state changes in returned queue:

- `Skip`: remove the active pending item.
- `Defer`: move the active pending item to the back of the queue.
- `Resolve`: remove or mark the active pending item as manually resolved,
  depending on the helper data model chosen in M10-105. The choice must be
  covered by tests and reflected in M10-106 event data.

Forbidden behavior:

- mutating `GameState` directly,
- appending `GameState.event_log` directly,
- publishing network payloads,
- exposing hidden source ids,
- executing unsupported card text.

## M10-106 Event Boundary

The committed event must record enough data to replay the queue commit:

- queue id,
- pending id,
- decision id,
- decision type,
- player index,
- before/after queue ids or stable queue hashes,
- manual unsupported marker for `Resolve` when applicable.

The event must not include hidden source card identity for perspectives that
should not see it.

## UI, Bot, And Network Rules

- PlayTable may request commit only through a command/facade path once it
  exists.
- Bot may choose a decision only from legal action/query output once exposed.
- Network may transport committed events after masking, but must not directly
  mutate local `GameState`.
- Session preview logs remain session-only and are never a replay source.

## Verification Requirements

M10-105 and M10-106 must add tests for:

- accepted `Skip`, `Defer`, and `Resolve` queue results,
- rejected missing queue, missing decision, invalid decision type, and pending
  id mismatch,
- input queue no-mutation,
- replayable event metadata,
- hidden-source masking,
- no direct UI/bot/network `GameState` mutation.
