# Trigger Check Log Summary Formatter Spec

## Status

Implemented in `M10-37`.

## Purpose

Extract PlayTable trigger-check replay log summary text construction into a pure
formatter/helper.

This keeps the current side-panel log summary text unchanged while removing
payload decoding and display-string branching from `PlayTableBootstrap`.

## Inputs

- trigger-check replay log payload list
- latest payload
- decoded trigger-check replay log
- latest trigger-check log entry

## Output

- no logs: `Trigger logs: 0`
- invalid latest payload:
  `Trigger logs: <count>\nLatest: invalid (<reason>)`
- decoded log with no entries:
  `Trigger logs: <count>\nLatest: empty`
- latest entry with summary:
  `Trigger logs: <count>\nLatest: <summary>`
- latest entry without summary:
  `Trigger logs: <count>\nLatest: <check_source> check <check_index> <trigger_type>`

## Boundary

The formatter must not:

- send payloads
- publish logs
- inspect hidden card identity beyond already-decoded replay log entries
- inspect deck order
- move cards
- apply trigger bonuses to `GameState`
- append to `GameState.event_log`
- mutate `GameState`

## Acceptance Tests

- no logs formats the existing `Trigger logs: 0` text
- invalid latest payload formats the existing invalid message
- empty decoded log formats the existing empty message
- latest summary text is used when present
- fallback check/source/type text is used when summary is blank
- PlayTable uses the formatter without changing summary text
- Unity compile and EditMode tests pass

## Future Extensions

- configurable visible payload count
- localized summary strings
- structured view model for richer trigger-check log detail panels
