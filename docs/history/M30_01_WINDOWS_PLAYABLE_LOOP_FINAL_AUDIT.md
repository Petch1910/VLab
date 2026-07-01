# M30-01 Windows Playable Loop Final Audit

Date: 2026-06-28

## Scope

Audit the Windows-first playable loop after M28 and M29 are complete.

## Evidence Reviewed

- `docs/WINDOWS_PLAYABLE_LOOP_CHECKLIST.md`
- `docs/WINDOWS_LOCAL_BUILD_KNOWN_LIMITATIONS.md`
- `client/unity/VanguardThaiSim/work/unity_compile_m28_10_match_log_density.log`
- `client/unity/VanguardThaiSim/work/unity_editmode_m28_10_match_log_density.xml`
- `client/unity/VanguardThaiSim/work/client_smoke_m28_10_match_log_density.log`
- `client/unity/VanguardThaiSim/work/windows_build_m28_10_match_log_density.log`
- `client/unity/VanguardThaiSim/work/player_smoke_m28_10_match_log_density.json`
- `client/unity/VanguardThaiSim/Assets/Scripts/Vanguard/UI/HomeLobbyBootstrap.cs`

## Passed

- Home loads pack/deck/status and routes to deck builder, solo setup, online
  room, manual, and settings.
- Deck Builder validates a playable `50+4` deck and round-trips deck code.
- Local PlayTable smoke covers setup, phase flow, call, attack, guard, Drive
  Check, Damage Check, End phase, and replay determinism.
- PlayTable primary panel is less dense after M28-10: selected-card preview
  remains primary, compact match log is primary, full match log is Advanced.
- Online Room has navigation lockout, reconnect text polish, quick deck
  selection/edit, no deck-code leak, and local deck readiness guard.
- Latest Windows player smoke reports `blockers=[]`.

## Blockers

### B1: Replay Home Route Is Still Locked

The checklist requires Replay to be reachable from the Windows loop, but
`HomeLobbyBootstrap` still wires the Home Replay button to `ShowReplayLocked()`
and displays a scheduled/polish placeholder message.

This is now the highest-value next slice because replay engine foundations
already exist (`GameReplay`, `GameReplayPlayer`, event log formatting, replay
determinism tests), but the player-facing Windows entry flow is still missing.

## Polish

- Home information architecture still reads as `Fight / Quick Start / Status`
  rather than the checklist's later `Card Workshop / Battle Center / System &
  Support` model. This is polish, not a blocker, because routes are usable.
- Card Workshop split-screen/drag wording in the checklist is aspirational.
  Current click-based builder/editor workflow is usable and tested.
- Fullscreen/borderless wording should stay deferred until visual QA shows a
  real usability problem.

## Deferred

- Android/mobile/APK/release work remains closed.
- Public release remains closed.
- CPU/bot strength work remains separate from this Windows loop audit.
- Authoritative server/ranked security remains out of scope for trusted-client
  Photon rooms.

## Decision

Next target: `M30-02` Windows Replay entry/browser slice.

The minimum acceptable slice should unlock the Replay button into a
player-facing Replay screen that can load at least a local/sample replay or
clearly state when no replay file is available. It must not change replay event
semantics or network protocols.
