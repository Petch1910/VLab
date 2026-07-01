# Pending Auto Ability Manual Resolution Session Apply Preview Spec

## Status

Implemented in `M10-97`.

## Purpose

Add a session-level helper that previews applying the latest pending AUTO
ability manual resolution decision to the latest pending AUTO ability queue
payload.

This milestone stores the returned queue payload in session history only. It
does not publish the queue payload, resolve card effects, or mutate `GameState`.

## Behavior

- Decodes the latest stored pending AUTO ability queue payload.
- Decodes and validates the latest stored manual resolution decision payload.
- Runs `PendingAutoAbilityManualResolutionApplyExecutor`.
- On accepted apply preview, encodes and stores the returned queue as a new
  pending queue payload in session history.
- On rejected apply preview, stores nothing and returns the rejected result.

## Boundary

This milestone must not:

- send network payloads
- apply card effects
- mutate `GameState`
- mutate source queue or decision payloads

## Acceptance Tests

- missing queue payload is rejected
- missing decision payload is rejected
- valid skip/defer preview stores a new queue payload
- resolve preview stores an unchanged queue payload with accepted result
- preview does not publish payloads or mutate `GameState`
