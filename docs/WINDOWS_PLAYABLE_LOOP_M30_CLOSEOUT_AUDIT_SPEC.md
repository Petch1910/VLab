# Windows Playable Loop M30 Closeout Audit Spec

Milestone: `M30-06`

## Purpose

Audit the Windows playable loop after the Replay export/import/viewer path is
available, then decide the next implementation target from evidence instead of
opening another large system by default.

## Scope

- Home navigation.
- Deck Builder readiness.
- Manual PlayTable local loop.
- Replay export, local import, and preview loop.
- Manual, Settings, Online Room entry, and known limitations.

## Boundaries

- Docs/audit first unless a blocker is found.
- No Android/mobile/APK/release work.
- No comparator asset/code/data copying.
- No network protocol rewrite.
- No bot/automation expansion unless the audit proves Windows UX/core flow is
  stable enough.

## Verification Plan

- Use latest M30-05 compile/EditMode/client smoke/Windows build/player smoke
  evidence.
- If audit triggers a runtime code change, rerun the relevant Windows-only
  verification profile.

## Expected Output

- A closeout audit in `docs/history/`.
- A clear next target: bug/polish pass, PlayTable UX pass, Online Room polish,
  or deferred feature track.

## Closeout - 2026-06-28

Audit result:

- No automated Windows-loop blocker remains after M30-05.
- Replay loop is now connected end to end:
  PlayTable local export -> local replay JSON -> Replay screen load -> preview.
- The next highest-risk area is no longer missing route coverage; it is player
  experience clarity and visual/information architecture quality.

Decision:

- Close `M30`.
- Start `M31-01` Windows UI evidence capture and blocker/polish audit using
  `docs/UI_EXPERIENCE_REDESIGN_SPEC.md` and current Windows player behavior as
  the source of truth.
