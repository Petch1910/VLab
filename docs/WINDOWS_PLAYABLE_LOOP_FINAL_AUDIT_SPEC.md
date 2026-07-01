# Windows Playable Loop Final Audit Spec

Milestone: `M30-01`

## Purpose

Audit the Windows-first playable loop after M28 and M29 are complete, before
opening another feature track.

The audit should verify that the program is coherent as a Windows simulator,
not just that isolated smoke tests pass.

## Scope

Review these flows:

- Home startup status and navigation.
- Deck Builder load/save/import/export readiness.
- Local PlayTable setup, turn flow, battle flow, selected-card preview, compact
  match log, Advanced drawer, and event readability.
- Online Room lobby readiness, deck guard, reconnect text, and table handoff.
- Manual, Settings, and known limitations visibility.
- Custom pack/import UX readiness at the current scope.

## Non-Goals

- No Android/mobile/APK/release work.
- No public release.
- No transport switch.
- No comparator asset/code/data copy.
- No new bot/ability automation unless the audit finds a blocker tied to the
  existing Windows flow.

## Output

- Audit findings grouped by blocker / polish / deferred.
- Decision on whether the next work should be a bugfix slice, UX polish slice,
  or a new feature track.
- Verification evidence paths from latest compile, EditMode, client smoke, and
  Windows player smoke.
