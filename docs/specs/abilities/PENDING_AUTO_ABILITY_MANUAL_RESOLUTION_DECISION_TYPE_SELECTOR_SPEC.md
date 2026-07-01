# Pending Auto Ability Manual Resolution Decision Type Selector Spec

## Status

Implemented in `M10-88`.

## Purpose

Allow online PlayTable users to choose the manual resolution decision type used
by `DraftDec`.

This milestone only changes draft metadata selection. It does not publish or
apply decisions.

## Behavior

- Online PlayTable shows `DecType`.
- `DecType` cycles deterministically:
  `Resolve` -> `Skip` -> `Defer` -> `Resolve`.
- Invalid or empty selector state falls back to `Resolve`.
- `DraftDec` uses the currently selected decision type.
- Local PlayTable does not show `DecType`.

## Boundary

This milestone must not:

- publish payloads
- resolve, skip, or defer abilities
- mutate `GameState`
- change stored request or decision payloads except when `DraftDec` explicitly
  creates a new draft

## Acceptance Tests

- selector helper cycles through supported decision types
- online PlayTable exposes `DecType`
- local PlayTable does not expose `DecType`
- `DraftDec` uses the selected decision type
- selector clicks do not publish payloads or mutate `GameState`
