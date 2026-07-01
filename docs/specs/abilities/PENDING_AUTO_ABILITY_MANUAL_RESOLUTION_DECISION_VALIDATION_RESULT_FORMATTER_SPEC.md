# Pending Auto Ability Manual Resolution Decision Validation Result Formatter Spec

## Status

Implemented in `M10-91`.

## Purpose

Format pending AUTO ability manual resolution decision validation results for
debug/status surfaces.

This milestone only centralizes text formatting. It does not validate payloads
directly, apply decisions, publish payloads, or mutate `GameState`.

## Behavior

- Accepted validation results format as:
  `Pending manual decision validation: valid type=<type> id=<pending-id> source=<source>`
- Rejected validation results format as:
  `Pending manual decision validation: rejected <reason>`
- Null validation results format as:
  `Pending manual decision validation: none`
- Hidden-source accepted results format source as `hidden`.

## Boundary

This milestone must not:

- run validation directly
- apply decisions to pending queues
- publish payloads
- mutate `GameState`
- reveal hidden source card identity

## Acceptance Tests

- accepted validation result formats decision type and pending id
- rejected validation result formats rejection reason
- null validation result formats a stable fallback
- hidden-source accepted result does not leak source identity
