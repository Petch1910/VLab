# M27-06 Windows PlayMode Integration Closeout

## Status

Complete.

## What Changed

- Added the first PlayMode test assembly:
  `Assets/Tests/PlayMode/VanguardThaiSim.PlayModeTests.asmdef`.
- Added `WindowsPlayableLoopPlayModeTests`.
- Covered the runtime path:
  Home screen -> in-memory smoke deck -> PlayTable -> Stand -> Draw -> Ride
  phase -> End phase.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m27_06_playmode_integration_r3.log`
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m27_06_playmode_integration.xml`
  passed `1127/1127`
- Unity PlayMode:
  `client/unity/VanguardThaiSim/work/unity_playmode_m27_06_playmode_integration_r2.xml`
  passed `1/1`

The first PlayMode attempt correctly failed because the `Draw` button is
disabled during `Mulligan`. The test was adjusted to follow the real UI legal
action mask by clicking `Stand` before `Draw`.

## Next Target

`M27-07`: Known limitations list for the local Windows build.
