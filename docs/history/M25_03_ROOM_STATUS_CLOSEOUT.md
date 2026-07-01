# M25-03 Room Status Closeout

## Status

Done.

## Scope Completed

- Added `docs/specs/multiplayer/WINDOWS_ROOM_STATUS_SPEC.md`.
- Added player-facing room status formatting for:
  - connection status
  - connected player count
  - deck hash readiness
  - pack hash match/mismatch
  - public/event cursor
- Wired the Windows Online Room panel to show the status block instead of raw
  debug payloads.
- Kept deck codes and revealed deck codes hidden from status text.
- Preserved Photon Realtime trusted-client room behavior and did not change
  transport payloads, hidden-state policy, or room lifecycle semantics.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m25_03_room_status.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m25_03_room_status.xml`
  passed `1063/1063`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m25_03_room_status.log`
  succeeded with `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m25_03_room_status.json`
  passed with `blockers=[]`.

## Next Target

`M25-04`: Reconnect UX with clear failure reasons.
