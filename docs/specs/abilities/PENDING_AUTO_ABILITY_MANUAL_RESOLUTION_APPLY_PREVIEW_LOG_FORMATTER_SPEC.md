# Pending Auto Ability Manual Resolution Apply Preview Log Formatter Spec

## Status

Implemented in `M10-100`.

## Purpose

Add pure formatter helpers for pending AUTO ability manual resolution apply
preview log entries.

This milestone is display-only. It does not store preview logs in session
history, show them in PlayTable, publish payloads, write to `GameState.event_log`,
resolve card effects, or mutate `GameState`.

## Behavior

- Format a single apply preview log entry as:
  - accepted decision type and pending id
  - rejected reason
  - null fallback
- Format a bounded newest-first list of apply preview log entries.
- Do not display free-text `summary` content because future summaries may carry
  source context. The formatter only displays safe structured fields.

## Boundary

This milestone must not:

- add session storage
- add PlayTable surfaces
- send network payloads
- write to `GameState.event_log`
- resolve card effects
- mutate entries or `GameState`

## Acceptance Tests

- accepted entry formats decision type and pending id
- rejected entry formats rejection reason
- null and empty list fallbacks are deterministic
- lists are newest-first and bounded
- formatter does not display free-text summary source context
- formatting does not mutate source entries
