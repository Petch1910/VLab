# M30-06 Windows Playable Loop Closeout Audit

Date: 2026-06-28

## Scope

Audit the Windows playable loop after `M30-05` connected local PlayTable replay
export with the Home Replay import/preview flow.

## Evidence Reviewed

- `docs/WINDOWS_PLAYABLE_LOOP_CHECKLIST.md`
- `docs/WINDOWS_PLAYABLE_LOOP_FINAL_AUDIT_SPEC.md`
- `docs/WINDOWS_REPLAY_ENTRY_BROWSER_SPEC.md`
- `docs/WINDOWS_REPLAY_LOCAL_FILE_IMPORT_SPEC.md`
- `docs/WINDOWS_REPLAY_VIEWER_LAUNCH_SPEC.md`
- `docs/WINDOWS_PLAYTABLE_REPLAY_EXPORT_SPEC.md`
- `client/unity/VanguardThaiSim/work/unity_compile_m30_05_playtable_replay_export.log`
- `client/unity/VanguardThaiSim/work/unity_editmode_m30_05_playtable_replay_export.xml`
- `client/unity/VanguardThaiSim/work/client_smoke_m30_05_playtable_replay_export.log`
- `client/unity/VanguardThaiSim/work/windows_build_m30_05_playtable_replay_export.log`
- `client/unity/VanguardThaiSim/work/player_smoke_m30_05_playtable_replay_export.json`

## Passed

- Home, Deck Builder, Manual, Settings, Online Room, and PlayTable remain
  covered by client/player smoke.
- Deck Builder still validates a playable `50+4` smoke deck and round-trips
  deck code.
- Local PlayTable still completes the Windows gameplay smoke route.
- Replay is no longer a locked Home route.
- Replay screen can load a local replay JSON path and preview it through
  `GameReplayPlayer`.
- Local PlayTable can export `vanguard_latest_replay.json` under the Unity
  project `work/` folder.
- Latest verification reports:
  - EditMode `1160/1160`
  - client smoke `blockers=[]`
  - Windows build `errors=0 warnings=0`
  - Windows player smoke `blockers=[]`

## Blockers

None found from the current automated Windows verification profile.

## Polish / Product Gaps

- Replay export is currently in the PlayTable Advanced drawer and writes a
  deterministic path. This is acceptable for the current slice, but a future
  player-facing replay browser should list local replay files or provide a
  clearer copy/open affordance.
- Replay import currently uses a raw path input. This keeps the system simple
  and testable, but it is not the final UX.
- Home/Card Browser/Deck Builder/PlayTable still need a visual and information
  architecture pass. The project has working flows, but user feedback and
  `docs/UI_EXPERIENCE_REDESIGN_SPEC.md` both show the UI can still read like an
  internal tool.
- Full manual ability automation, CPU strength, and advanced bot work remain
  gated behind core correctness and Windows UX stability.

## Deferred

- Android/mobile/APK/release work.
- Public distribution.
- Authoritative server/ranked multiplayer.
- Comparator asset/code/data copying.

## Decision

Close `M30`.

Next target: `M31-01` Windows UI evidence capture and blocker/polish audit.

Rationale: the playable loop is now technically connected on Windows. The next
best work is not another subsystem; it is verifying the player-facing UI with
current screenshots/Windows player behavior and prioritizing concrete fixes
against `docs/UI_EXPERIENCE_REDESIGN_SPEC.md`.
