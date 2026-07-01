# Pending Auto Ability Manual Resolution Apply Preview PlayTable Surface Spec

Status: Implemented in M10-102.

## Purpose

Show the pending auto ability manual resolution apply-preview session log on the
PlayTable so a tester can see the latest preview attempt and a short bounded
history without committing the preview result.

This surface is for human/debug visibility only. It is not a gameplay commit
path.

## Inputs

- `MultiplayerGameSessionController.LatestPendingAutoAbilityManualResolutionApplyPreviewLog`
- `MultiplayerGameSessionController.PendingAutoAbilityManualResolutionApplyPreviewLogs`
- `PendingAutoAbilityManualResolutionApplyPreviewLogFormatter`

## UI Output

The PlayTable side panel exposes two read-only text surfaces:

- `Pending Ability Manual Resolution Apply Preview Log Latest`
- `Pending Ability Manual Resolution Apply Preview Log List`

Local/manual table mode must render the formatter zero-state messages:

- `Pending manual decision apply preview log: none`
- `Pending manual decision apply preview log: 0`

Online mode must update after the `ApplyDec` preview action refreshes the
session state.

## Safety Rules

- The UI must not mutate `GameState`.
- The UI must not write to `GameState.event_log`.
- The UI must not publish any network payload.
- The UI must not display hidden source card ids or source instance ids.
- The bounded list must be newest-first and use the formatter's safe summary.

## Verification

EditMode coverage must verify:

- local PlayTable zero-state text,
- online accepted apply-preview log rendering,
- online rejected apply-preview log rendering,
- no hidden source identifiers in apply-preview log text,
- no network publish from preview rendering,
- no `GameState` or event-log mutation.
