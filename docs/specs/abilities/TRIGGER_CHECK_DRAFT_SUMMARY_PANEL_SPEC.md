# Trigger Check Draft Summary Panel Spec

## Status

Implemented in `M10-23`.

## Purpose

Show a compact read-only PlayTable side-panel summary of the current manual
trigger-check draft metadata before publishing.

The summary helps users see the selected draft trigger type, check source, and
check index without sending network payloads or mutating game state.

## Inputs

- current local draft trigger type
- current local draft check source
- current local draft check index

## Output

- side-panel `Trigger Draft Summary` text
- online default: `Draft: Unknown / Manual / idx 0`
- local default: `Draft: local mode`
- summary updates when `TrigType`, `ChkSrc`, or `ChkIdx` cycles

## Boundary

The summary panel must not:

- send payloads
- inspect deck order
- move cards
- apply trigger bonuses to `GameState`
- append to `GameState.event_log`

## Acceptance Tests

- online table shows the default draft summary
- cycling type/source/index updates the summary deterministically
- summary updates do not send payloads
- local table returns local-mode summary text
- `GameState` is unchanged after summary updates
- Unity compile and EditMode tests pass

## Future Extensions

- style the summary as a compact draft status band
- include selected card short id when a card is selected
- add explicit check sequence labels for multi-check flows
