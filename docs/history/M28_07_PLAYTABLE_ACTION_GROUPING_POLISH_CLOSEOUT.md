# M28-07 PlayTable Action Grouping Polish Closeout

## Scope

`M28-07` improves PlayTable scanability without moving buttons or changing
RulesCore. The change adds a small player-facing action group legend to explain
which buttons belong to turn flow, card movement, and battle/check flow.

This deliberately avoids a larger button-row rewrite until the Windows table
has more visual QA evidence.

## Implementation

- Added `PlayTableActionGroupLegendFormatter`.
- Added a `PlayTable Action Group Legend` side-panel surface.
- Kept all existing buttons, labels, callbacks, and legal-action gating intact.
- Added EditMode coverage through `PlayTableShowsActionGroupLegendSurface`.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m28_07_action_group_legend.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m28_07_action_group_legend.xml`
  passed `1137/1137`.
- Unity PlayMode:
  `client/unity/VanguardThaiSim/work/unity_playmode_m28_07_action_group_legend.xml`
  passed `2/2`.
- Client smoke runner:
  `client/unity/VanguardThaiSim/work/client_smoke_m28_07_action_group_legend.log`
  passed with `blockers=[]`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m28_07_action_group_legend.log`
  succeeded with `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m28_07_action_group_legend.json`
  passed with `blockers=[]`.

## Result

`M28-07` is complete. The PlayTable now has both a dynamic next-action hint and
a stable action-group legend while preserving the manual simulator command
surface.

## Next Target

`M28-08` should audit PlayTable side-panel density and visual fit after the new
guidance text. If it finds overflow or cramped scan paths, the next code slice
should reduce density before adding more gameplay systems.
