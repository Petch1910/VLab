# M28-05 PlayTable Guided Next-Action Panel Closeout

## Scope

`M28-05` adds a player-facing next-action hint to the Windows PlayTable. The
goal is to reduce confusion from dense manual simulator controls without
rewriting the PlayTable layout or changing RulesCore behavior.

## Implementation

- Added `PlayTableGuidedNextActionFormatter`.
- Added a `Next Action` side-panel surface named
  `PlayTable Guided Next Action`.
- Added `PlayTableBootstrap.CreateGuidedNextAction()` for tests and future
  smoke checks.
- Hints are based on the display-state view and current local seat:
  setup, first Vanguard, mulligan/stand, ride, main/call, battle/attack,
  guard/check handoff, trigger checks, and end cleanup.
- The formatter does not mutate `GameState`, does not execute actions, and does
  not inspect unmasked opponent private state.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m28_05_guided_next_action.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m28_05_guided_next_action.xml`
  passed `1136/1136`.
- Unity PlayMode:
  `client/unity/VanguardThaiSim/work/unity_playmode_m28_05_guided_next_action.xml`
  passed `2/2`.
- Client smoke runner:
  `client/unity/VanguardThaiSim/work/client_smoke_m28_05_guided_next_action.log`
  passed with `blockers=[]`.

## Result

`M28-05` is complete. The PlayTable now has a single player-facing line that
tells the current local seat what to do next while preserving the existing
manual freedom.

## Next Target

`M28-06` should rebuild the Windows player and run the built-player smoke
against `VanguardThaiSim.exe -vanguardPlayerSmoke`. This verifies the new
PlayTable guidance from the actual Windows executable path before more UI work.
