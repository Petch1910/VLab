# M25 Windows Online Room Usability Closeout

## Status

Done.

## Scope

Closed the Windows-first Online Room usability milestone for casual friend-room
play on Windows. Photon remains the trusted-client transport, and this milestone
does not claim ranked-server security.

## Completed Tasks

- `M25-01`: Photon trusted-client room policy.
- `M25-02`: Lobby flow for create, join, ready, start, rematch, and back home.
- `M25-03`: Room status for connection, player count, deck hash, pack hash, and
  public cursor.
- `M25-04`: Reconnect UX with clear failure reasons.
- `M25-05`: Online PlayTable default UI hides payload/debug details unless
  Advanced is open.
- `M25-06`: Player-facing replay sync/status.
- `M25-07`: Online room test rollup for privacy, stale cursor, reconnect, and
  public event coverage.
- `M25-08`: Machine-checkable closeout report.

## Result

The Windows Online Room flow is now easier to reason about for friend-room play:
the lobby exposes player-facing ready/start/rematch/status text, reconnect
failures are explicit, online PlayTable keeps technical payloads out of the
default surface, and replay sync status is visible without exposing private
state.

## Guardrails Preserved

- No Android, APK, LDPlayer, mobile QA, app packaging, release candidate, or
  public distribution work was run.
- Photon trusted-client room transport remains in place.
- No transport switch was made.
- No comparator assets, code, data, icons, playmats, or pack files were copied.
- Hidden state and private deck information remain masked in player-facing,
  spectator, reconnect, and replay surfaces.
- UI/network/reporting code does not mutate `GameState` directly.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m25_08_online_room_closeout.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m25_08_online_room_closeout.xml`
  passed `1078/1078`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m25_08_online_room_closeout.log`
  succeeded with `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m25_08_online_room_closeout.json`
  passed with `blockers=[]`.

## Next Target

Proceed to `M26-01`: audit that `M21-M25` are complete before returning to
bot/automation work.
