# Trigger Check Replay Log Masking Spec

## Status

Implemented in `M10-10`.

## Purpose

Create owner-safe, opponent-safe, and spectator-safe views of
`TriggerCheckReplayLog` without mutating the source log.

Trigger check logs may contain checked card ids, checked instance ids, log ids,
and modifier ids that were derived from private or pre-public card information.
Masking must redact those identity fields while preserving public timing,
player, check source, trigger type, accepted/manual status, modifier count, and
notes.

## Inputs

- `TriggerCheckReplayLog`
- `GameStateViewPerspective`
- viewer player index
- reveal-owner-checks flag

## Output

- cloned replay log
- optionally redacted checked card identity
- optionally redacted modifier ids
- rebuilt summary and masked log entry ids

## Boundary

The helper must not:

- mutate the source log
- mutate `GameState`
- integrate with Photon payloads yet
- decide whether an actual live trigger check has happened
- reveal hidden checked card ids through log ids, summaries, or modifier ids

## Acceptance Tests

- true-state view preserves checked card identity
- owner player view can preserve own entries while masking opponent entries
- spectator/public view masks checked card id, checked instance id, log id, and
  modifier ids
- masked logs JSON round-trip
- masking is deterministic and does not mutate the source log or `GameState`

## Future Extensions

- Photon payload codec for masked trigger check logs
- replay UI that switches true/player/spectator views
- policy hook for formats where trigger checks are always public immediately
