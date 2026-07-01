# M25-06 Replay Sync Status Closeout

## Status

Done.

## Scope Completed

- Added `docs/specs/multiplayer/ONLINE_REPLAY_SYNC_STATUS_SPEC.md`.
- Added `PlayTableReplaySyncStatusFormatter` for player-facing online replay
  sync text.
- Online PlayTable match log now prepends replay sync status when running in an
  online session.
- Added reconnect apply result text for public replay batches.
- Preserved public replay application, reconnect protocol, payload formats,
  Photon transport, and hidden-state masking.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m25_06_replay_sync_status.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m25_06_replay_sync_status.xml`
  passed `1071/1071`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m25_06_replay_sync_status.log`
  succeeded with `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m25_06_replay_sync_status.json`
  passed with `blockers=[]`.

## Next Target

`M25-07`: Online room test rollup for no deck-code leak, stale cursor reject,
reconnect display, and masked event delivery.
