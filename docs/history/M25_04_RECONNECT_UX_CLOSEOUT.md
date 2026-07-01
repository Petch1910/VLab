# M25-04 Reconnect UX Closeout

## Status

Done.

## Scope Completed

- Added `docs/specs/multiplayer/WINDOWS_RECONNECT_UX_SPEC.md`.
- Improved reconnect summary text for:
  - no pending reconnect work
  - request cursor and reason
  - batch room mismatch
  - missing room state
  - cursor match
  - cursor behind local state
  - cursor gap that would skip events
- Added Start Table rejection formatting for reconnect cursor mismatch, room
  mismatch, and replay failure cases.
- Preserved the existing reconnect protocol and kept cursor-gap rejection
  semantics unchanged.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m25_04_reconnect_ux_b.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m25_04_reconnect_ux_b.xml`
  passed `1066/1066`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m25_04_reconnect_ux.log`
  succeeded with `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m25_04_reconnect_ux.json`
  passed with `blockers=[]`.

## Next Target

`M25-05`: Online PlayTable primary UI must not show payload/debug by default.
