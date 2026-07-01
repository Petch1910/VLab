# M28-02 Local PlayTable Seat Toggle Closeout

## Status

Done.

## Scope

Added a local manual PlayTable seat toggle so a Windows player can switch the
visible/control perspective between P1 and P2 without recreating the game
state. Online PlayTable sessions remain locked to the session local player.

This is a player-facing Windows UX foundation for a later two-seat UI match
smoke. It does not expand card automation, bot search, networking authority, or
mobile/app packaging.

## Code Changes

- Added a toolbar `Seat P2` / `Seat P1` control in local PlayTable mode.
- Online PlayTable mode changes the control to `Seat Lock`, disables it, and
  leaves `CurrentPlayerIndex` unchanged even if the handler is invoked.
- Switching local seats clears selected card, selected target, and pending AUTO
  selection UI state, then refreshes the table for the selected seat.
- Added tests for:
  - runtime UI creation including the seat button,
  - local P1/P2 switching without committed events,
  - online seat-switch lockout without network/game-state mutation.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m28_02_local_seat_toggle.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m28_02_local_seat_toggle.xml`
  passed `1131/1131`.
- Unity PlayMode:
  `client/unity/VanguardThaiSim/work/unity_playmode_m28_02_local_seat_toggle.xml`
  passed `1/1`.
- Client smoke runner:
  `client/unity/VanguardThaiSim/work/client_smoke_m28_02_local_seat_toggle.log`
  passed with `blockers=[]`.

## Guardrails Preserved

- No Android, APK, LDPlayer, mobile QA, app packaging, release candidate, or
  public distribution work was run.
- No comparator assets, code, icons, playmats, card data, or pack files were
  copied.
- Seat switching does not execute a `GameActionType` and does not append
  `GameState.event_log`.
- Online sessions cannot change local player control through the local seat
  toggle.

## Follow-Up

Proceed to `M28-03`: UI-level two-seat match smoke. The next PlayMode test
should use the new seat toggle to drive both players through first Vanguard
setup, P1 turn flow, P2 guard/check, and End phase using the actual runtime UI.
