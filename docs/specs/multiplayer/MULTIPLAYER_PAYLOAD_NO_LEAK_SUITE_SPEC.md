# Multiplayer Payload No-Leak Suite Spec

## Milestone

`M18-07`

## Goal

Create an explicit regression-suite inventory for multiplayer payloads and
hidden-information boundaries. This gate protects owner-private/trusted-client
online play before release artifact work begins.

## Scope

`MultiplayerPayloadNoLeakSuiteReportBuilder.CreateCurrent()` returns a report
with these required categories:

- `command_envelope_cursor`: player id, sequence, room game id, state cursor,
  stale cursor rejection, out-of-turn rejection, and ownership mismatch
  rejection.
- `owner_private_room_state`: owner-private local true state initialization,
  opponent placeholder construction, commitment mismatch rejection, and public
  count metadata requirements.
- `public_event_masking`: true `GameEvent` to public event conversion,
  hidden draw masking, public reveal identity, and reveal proof metadata.
- `public_reconnect_recovery`: public reconnect batch creation, application,
  cursor mismatch rejection, and commitment-only true reconnect blocking.
- `spectator_replay_sync`: public replay player and visible event log cloning.
- `trigger_check_payload`: trigger-check draft/replay payload masking,
  deterministic encoding, Photon wrapping, transport hook, and no-mutation.
- `pending_auto_queue_payload`: pending AUTO queue payload masking,
  deterministic encoding, Photon wrapping, transport hook, and no-mutation.
- `pending_auto_resolution_request_payload`: pending AUTO resolution request
  hidden-source sanitization and no-mutation.
- `manual_resolution_decision_payload`: manual resolution decision hidden-source
  sanitization, Photon wrapping, transport hook, and no-mutation.
- `session_storage_no_mutation`: diagnostic payload receipt stays outside
  `GameState` and normal event sync still works.

## Non-Goals

- No custom authoritative server work.
- No ranked security guarantee.
- No new Photon live smoke run requirement beyond existing smoke runners.
- No transport behavior change unless a focused no-leak gap is found.

## Verification

- `MultiplayerPayloadNoLeakSuiteReportTests.CurrentReportValidatesRequiredCategories`
- `MultiplayerPayloadNoLeakSuiteReportTests.MissingRequiredCategoryRejectsReport`
- `MultiplayerPayloadNoLeakSuiteReportTests.RequiredCategoryWithoutRepresentativeTestsRejectsReport`
- `MultiplayerPayloadNoLeakSuiteReportTests.ReportJsonRoundTripKeepsMilestoneAndCategoriesVisible`
- Unity compile passes.
- Unity EditMode tests pass.

## Next Target

After this gate passes, continue with `M18-08 Windows build artifact`.
