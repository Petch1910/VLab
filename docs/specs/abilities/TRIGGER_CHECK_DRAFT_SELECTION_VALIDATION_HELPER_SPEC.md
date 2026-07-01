# Trigger Check Draft Selection Validation Helper Spec

## Status

Implemented in `M10-31`.

## Purpose

Extract PlayTable manual trigger-check draft publish selection validation into a
pure helper with explicit accepted/rejection output.

This keeps the existing `DraftTrig` publish behavior and user-facing messages
unchanged while making validation reusable for future draft UI surfaces.

## Inputs

- online/local mode flag
- selected card instance id
- selected card id

## Output

- `accepted`
- `rejection_reason`

## Boundary

The helper must not:

- know about Unity UI objects
- know about multiplayer sessions or transports
- send payloads
- move cards
- inspect hidden card identity beyond the public hidden sentinel
- inspect deck order
- apply trigger bonuses to `GameState`
- append to `GameState.event_log`
- mutate `GameState`

## Acceptance Tests

- local mode rejects with the existing online-only message
- no selection rejects with the existing select-card message
- hidden card id rejects with the existing hidden-identity message
- visible selected card is accepted
- PlayTable uses the helper without changing behavior
- Unity compile and EditMode tests pass

## Future Extensions

- validation result codes for localized UI
- disabled-control tooltip mapping from validation failures
- shared validation for manual draft and real trigger-check UI
