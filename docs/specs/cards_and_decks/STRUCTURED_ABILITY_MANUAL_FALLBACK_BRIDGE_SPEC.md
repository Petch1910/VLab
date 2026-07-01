# Structured Ability Manual Fallback Bridge Spec

## Status

Implemented in `M12-11`.

## Purpose

Connect unsupported structured ability execution paths to the existing pending
AUTO manual resolution artifacts instead of leaving them as disconnected
rejections.

The bridge is intentionally pure. It does not mutate `GameState`, queues,
session storage, UI, or network state.

## Runtime Surface

`StructuredAbilityManualFallbackBridge.CreateResolveDecision(...)` creates:

- `PendingAutoAbility`
- `PendingAutoAbilityResolutionRequest`
- `PendingAutoAbilityManualResolutionDecision` with decision type `Resolve`

The bridge uses the existing factories:

- `PendingAutoAbilityResolutionRequestFactory`
- `PendingAutoAbilityManualResolutionDecisionFactory`

## Inputs

Required:

- `StructuredAbility`
- rejection reason from a structured ability fixture/template path
- player index
- timing event/window
- hidden-source flag

The bridge rejects if:

- ability is missing
- `manual_fallback` is false
- rejection reason is empty
- existing request/decision factory rejects

## Hidden Source Boundary

When `hideSourceCardIdentity = true`:

- request and decision source instance ids are empty
- request and decision source card id is `GameStateViewFactory.HiddenCardId`
- serialized bridge output does not include the real card id

## Boundary

M12-11 does not:

- mutate `GameState`
- append `GameEvent`
- mutate pending AUTO queue state
- publish network payloads
- auto-resolve unsupported card text
- treat manual fallback as automated effect success

## Verification

EditMode coverage verifies:

- unsupported fixture result creates a Resolve decision
- hidden fallback does not leak source identity
- `manual_fallback = false` rejects
- missing reason rejects
- bridge creation does not mutate source ability or source `GameState`
- result JSON round-trip

## Next Work

`M12-12` should close M12 with regression runs, docs status, and a clear M13
next target.
