# Trigger Check Log Publish Online Validation Helper Spec

## Status

Implemented in `M10-38`.

## Purpose

Extract the PlayTable online-only guard for `TrigLog` publish into a pure
validation helper.

This keeps the existing offline rejection text unchanged while making trigger
check replay-log publish availability reusable for future compact or mobile
controls.

## Inputs

- whether an online multiplayer session is available

## Output

- accepted validation result when online
- rejected validation result when offline/local with:
  `Trigger log publish is only available online.`

## Boundary

The validator must not:

- send payloads
- decode trigger-check replay logs
- inspect card identity
- inspect deck order
- move cards
- apply trigger bonuses to `GameState`
- append to `GameState.event_log`
- mutate `GameState`

## Acceptance Tests

- offline/local mode is rejected with the existing message
- online mode is accepted with an empty rejection reason
- PlayTable uses the validator without changing offline user-facing text
- Unity compile and EditMode tests pass

## Future Extensions

- disabled-state tooltip text
- validation for missing stored trigger-check replay log payloads
- shared publish availability model for mobile controls
