# Pending Auto Ability Manual Resolution Apply Result Formatter Spec

## Status

Implemented in `M10-95`.

## Purpose

Format pending AUTO ability manual resolution apply results for future
PlayTable/session status surfaces.

This milestone only centralizes text formatting. It does not apply decisions,
mutate queues, publish payloads, or mutate `GameState`.

## Behavior

- Accepted result formats as:
  `Pending manual decision apply: accepted type=<type> id=<pending-id>`
- Rejected result formats as:
  `Pending manual decision apply: rejected <reason>`
- Null result formats as:
  `Pending manual decision apply: none`

## Boundary

This milestone must not:

- validate apply inputs directly
- mutate pending queues
- resolve abilities
- publish payloads
- mutate `GameState`

## Acceptance Tests

- accepted result formats decision type and pending id
- rejected result formats rejection reason
- null result formats a stable fallback
