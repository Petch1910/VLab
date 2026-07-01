# Trigger Check Log Publish Result Formatter Spec

## Status

Implemented in `M10-36`.

## Purpose

Extract PlayTable trigger-check replay log publish result messages into a pure
UI formatter.

This keeps `TrigLog` user-facing text stable while making the replay-log publish
surface easier to reuse in compact or mobile controls.

## Inputs

- trigger-check replay log publish transport result
- transport success flag
- transport error code
- transport message

## Output

- successful publish: `Published trigger check log.`
- failed publish: `<error_code>: <message>`
- null result: `TRANSPORT_ERROR: no result returned.`

## Boundary

The formatter must not:

- send payloads
- decode trigger-check replay logs
- inspect card identity
- inspect deck order
- move cards
- apply trigger bonuses to `GameState`
- append to `GameState.event_log`
- mutate `GameState`

## Acceptance Tests

- successful publish formats the existing success message
- failed publish preserves the existing `error_code: message` text
- null result formats a deterministic transport error message
- empty non-null result preserves the current concatenation behavior
- PlayTable uses the formatter without changing normal user-facing text
- Unity compile and EditMode tests pass

## Future Extensions

- localized result strings
- richer detail text for transport failures
- shared status presenter for trigger-check replay log and manual draft publish
