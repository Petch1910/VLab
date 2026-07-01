# PlayTable Event Log Formatter Spec

## Status

Implemented in `M10-42`.

## Purpose

Extract PlayTable event log text construction into a pure formatter.

This keeps replay/event feedback deterministic and covered by tests while
preserving the current newest-first event log display.

## Inputs

- game event list
- maximum visible entry count

## Output

- empty log: `Event log is empty.`
- newest-first event lines prefixed with one-based event numbers
- move-card lines: `#<n> <action_type> <from_zone>><to_zone>`
- gift marker lines: `#<n> AddGiftMarker <gift_marker_type> +<marker_delta>`
- phase-change fallback lines: `#<n> <action_type> <previous_phase>><new_phase>`

## Boundary

The formatter must not:

- inspect Unity UI objects
- execute legal actions
- move cards
- inspect deck order
- send multiplayer payloads
- append to `GameState.event_log`
- mutate `GameState`

## Acceptance Tests

- empty event log formats existing text
- move-card events preserve existing line format
- gift marker events preserve existing line format
- phase events preserve existing fallback line format
- event log is newest-first and limited by max visible count
- PlayTable uses the formatter without changing event log text
- Unity compile and EditMode tests pass

## Future Extensions

- compact mobile event log rows
- severity/category metadata for filtering
- structured event log view model for replay controls
