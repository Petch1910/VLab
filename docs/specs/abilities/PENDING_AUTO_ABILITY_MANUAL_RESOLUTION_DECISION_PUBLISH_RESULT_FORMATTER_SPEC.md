# Pending Auto Ability Manual Resolution Decision Publish Result Formatter Spec

## Status

Implemented in `M10-85`.

## Purpose

Format transport results from publishing pending AUTO ability manual resolution
decision payloads.

This milestone only centralizes UI status text. It does not change transport,
session storage, or decision application.

## Behavior

- Successful publish formats as
  `Published pending auto ability manual resolution decision.`
- Null result formats as `TRANSPORT_ERROR: no result returned.`
- Failed result formats as `<error_code>: <message>`.
- PlayTable `DecAuto` publish status uses this formatter.

## Boundary

This milestone must not:

- publish payloads directly
- resolve, skip, or defer pending abilities
- mutate `GameState`

## Acceptance Tests

- success result formats deterministic success message
- transport failure formats error code and message
- null result fallback is deterministic
- PlayTable publish status uses the formatter
