# Trigger Check PlayTable UI Surface Spec

## Status

Implemented in `M10-15`.

## Purpose

Display a compact read-only summary of received trigger check replay payloads in
the online PlayTable side panel.

This is a visibility surface only. It must not publish trigger logs, resolve
triggers, apply combat modifiers, or mutate `GameState`.

## Inputs

- `MultiplayerGameSessionController.TriggerCheckReplayLogPayloads`
- decoded `TriggerCheckReplayLog` summaries

## Output

- online mode summary includes trigger log count
- side panel trigger check summary text
- invalid payloads display as invalid without crashing

## Boundary

The UI surface must not:

- mutate `GameState`
- append to `GameState.event_log`
- send trigger check payloads
- resolve trigger checks
- expose unmasked hidden card identity beyond what the stored payload already
  contains

## Acceptance Tests

- online PlayTable shows trigger log count and latest summary
- displaying trigger logs does not mutate `GameState`
- local PlayTable reports no trigger logs
- Unity compile and EditMode tests pass

## Future Extensions

- scrollable trigger-check detail panel
- merge trigger check diagnostics into replay timeline UI
- manual trigger-log publishing controls were split into
  `TRIGGER_CHECK_MANUAL_PUBLISH_CONTROL_SPEC.md`
