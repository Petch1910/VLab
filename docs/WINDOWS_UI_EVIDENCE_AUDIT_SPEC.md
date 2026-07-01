# Windows UI Evidence Audit Spec

Milestone: `M31-01`

## Purpose

Review the current Windows player-facing UI from evidence before implementing
more features. The goal is to turn the user's "UI still feels weird" feedback
into concrete, ordered fixes.

## Source References

- `docs/UI_EXPERIENCE_REDESIGN_SPEC.md`
- `docs/UI_UX_SPEC.md`
- `docs/WINDOWS_PLAYABLE_LOOP_CHECKLIST.md`
- Latest Windows build:
  `client/unity/VanguardThaiSim/build/windows/latest/VanguardThaiSim.exe`
- Latest Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m30_05_playtable_replay_export.json`

## Screens To Review

- Home / lobby.
- Card Browser.
- Deck Builder / Card Workshop.
- PlayTable.
- Replay.
- Manual.
- Settings.
- Online Room.

## Boundaries

- Windows only.
- No Android/mobile/APK/release work.
- No comparator asset/code/data copying.
- No new network transport.
- No bot/automation expansion in this audit.

## Output

- A closeout audit in `docs/history/`.
- A prioritized next slice with a small code/test/docs scope.
- Known limitations remain explicit instead of being hidden behind polish.

## Closeout - 2026-06-28

Audit result:

- Automated Windows smoke covers functional flow, but it does not prove the
  UI feels clear to a player.
- The strongest player-facing evidence remains the earlier Card Browser /
  Deck Builder screenshot showing a visually confusing first screen.
- Existing `UI_EXPERIENCE_REDESIGN_SPEC.md` already identifies this as a
  structural UX issue: player goals must be clearer than implementation
  controls.

Decision:

- Start with Card Workshop first-screen clarity instead of adding a new
  subsystem.
- Next target: `M31-02` Card Workshop first-screen clarity pass.
