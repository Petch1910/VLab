# M28-03 UI-Level Two-Seat Match Smoke Closeout

## Scope

`M28-03` adds PlayMode coverage that drives the runtime PlayTable UI through a
two-seat local manual match smoke. The test uses UI buttons and display-state
assertions instead of mutating `GameState` directly.

The covered route is:

1. P1 sets first Vanguard from Ride Deck.
2. Local seat switches to P2.
3. P2 sets first Vanguard from Ride Deck.
4. Local seat switches back to P1.
5. P1 advances through Stand & Draw, Draw, Ride, Main, rear-guard call, Battle,
   and attack declaration.
6. Local seat switches to P2.
7. P2 guards and resolves Damage Check.
8. Local seat switches back to P1.
9. P1 resolves Drive Check and End phase.

## Implementation

- Updated `WindowsPlayableLoopPlayModeTests` with
  `LocalTwoSeatMatchSmokeUsesRuntimeUiControls`.
- Added runtime UI card-button lookup that supports both card-id-named buttons
  and generic board card face buttons whose child text contains the displayed
  card id.
- Kept the M28-02 local seat toggle behavior unchanged: local mode can switch
  seats, online mode remains seat-locked.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m28_03_two_seat_playmode_smoke.log`
  has no compiler-error markers.
- Unity PlayMode:
  `client/unity/VanguardThaiSim/work/unity_playmode_m28_03_two_seat_playmode_smoke_r2.xml`
  passed `2/2`.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m28_03_two_seat_playmode_smoke.xml`
  passed `1131/1131`.
- Client smoke runner:
  `client/unity/VanguardThaiSim/work/client_smoke_m28_03_two_seat_playmode_smoke.log`
  passed with `blockers=[]`; PlayTable smoke still reports
  `Windows gameplay completion smoke passed with 16 committed event(s).`

## Result

`M28-03` is complete. The Windows PlayTable now has a regression test that
proves a local two-seat manual match route can be driven through the visible UI.

## Next Target

`M28-04` should audit the Windows manual match flow gaps found by the M28-01 to
M28-03 gates before adding more features. The audit should list the remaining
player-facing blockers or polish targets in setup, turn flow, battle, guard,
check resolution, event readability, and table navigation.
