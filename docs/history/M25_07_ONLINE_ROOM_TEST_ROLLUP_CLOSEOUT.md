# M25-07 Online Room Test Rollup Closeout

## Status

Done.

## Scope

M25-07 adds a compact online-room coverage inventory for the Windows Online
Room usability pass. It does not change runtime multiplayer behavior, Photon
transport payloads, hidden-state rules, or room lifecycle.

## Implemented

- Added `docs/specs/multiplayer/ONLINE_ROOM_TEST_ROLLUP_SPEC.md`.
- Added `OnlineRoomTestRollupReport` as a serializable coverage inventory.
- Added EditMode coverage for:
  - deck identity privacy / no deck-code leak category
  - stale cursor rejection category
  - reconnect display category
  - masked public event delivery category
  - JSON round-trip without `deck_code`, `revealed_deck_code`, or sample revealed values.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m25_07_online_room_test_rollup.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m25_07_online_room_test_rollup.xml`
  passed `1074/1074`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m25_07_online_room_test_rollup.log`
  succeeded with `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m25_07_online_room_test_rollup.json`
  passed with `blockers=[]`.

## Notes

- The rollup report intentionally uses `deck_identity_privacy` instead of a
  category id containing `deck_code`, because the report JSON itself is tested
  for absence of that sensitive payload-field spelling.
- Next target: `M25-08` Windows Online Room usability closeout.
