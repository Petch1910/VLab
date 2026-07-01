# Windows Online Room Closeout Spec

Milestone: `M25-08`

## Purpose

Close the Windows Online Room usability pass with a compact, machine-checkable
acceptance report. The closeout proves the M25 work is complete enough to move
back to the bot/automation return gate without expanding scope.

## Required Coverage

The closeout report must include all completed M25 tasks:

- `M25-01`: Photon trusted-client room policy
- `M25-02`: lobby flow
- `M25-03`: room status
- `M25-04`: reconnect UX
- `M25-05`: online PlayTable default UI
- `M25-06`: replay sync/status
- `M25-07`: online room test rollup

It must also preserve the active guardrails:

- Photon remains the transport.
- No transport switch without ADR.
- Hidden state remains masked.
- Default online UI hides payload/debug details.
- Verification remains Windows-only.
- Comparator products remain reference-only.

## Scope

This milestone is a closeout/acceptance milestone. It may add a report model,
tests, and documentation, but it must not change runtime Photon behavior,
command validation, room lifecycle, hidden-state rules, or PlayTable gameplay.

## Verification

- EditMode tests validate every required M25 task appears in the closeout
  report.
- EditMode tests validate every required guardrail appears in the report.
- JSON round-trip must preserve the report and avoid sensitive payload-field
  spellings such as `deck_code`.
- Unity compile, EditMode, Windows build, and Windows player smoke remain green.
