# Trigger Check Draft Publish Result Message Formatter Spec

## Status

Implemented in `M10-32`.

## Purpose

Extract PlayTable manual trigger-check draft publish result message selection
into a pure UI formatter.

This keeps the current user-facing text unchanged while making result-message
logic reusable for future trigger-check UI surfaces.

## Inputs

- manual trigger-check draft publish result
- accepted flag
- sent flag
- rejection reason
- transport error code
- transport message

## Output

- accepted and sent result: `Published manual trigger draft.`
- otherwise the first non-empty value from:
  1. `rejection_reason`
  2. `transport_error_code`
  3. `transport_message`
- empty string when no result or no message is available

## Boundary

The formatter must not:

- send payloads
- inspect card identity
- inspect deck order
- move cards
- apply trigger bonuses to `GameState`
- append to `GameState.event_log`
- mutate `GameState`

## Acceptance Tests

- successful publish formats the existing success message
- rejection reason takes priority over transport fields
- transport error code is used when no rejection reason exists
- transport message is used when no rejection reason or error code exists
- null or empty result formats as an empty string
- PlayTable uses the formatter without changing user-facing text
- Unity compile and EditMode tests pass

## Future Extensions

- result codes for localized UI
- tooltip/detail text for transport failures
- shared result rendering for manual draft and real trigger-check publish flows
