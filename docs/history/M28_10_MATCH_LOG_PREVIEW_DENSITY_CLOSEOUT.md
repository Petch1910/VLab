# M28-10 Match Log / Preview Density Closeout

Date: 2026-06-28

## Scope

M28-10 reviews and reduces the remaining primary PlayTable side-panel density
after M28-09 moved Bot Plan into Advanced.

## Implemented

- Kept `Selected Card Preview` in the primary side panel.
- Changed the primary match log to `PlayTable Compact Match Log`.
- The compact log shows:
  - total event count
  - latest event
  - latest 3 recent events
  - `Full log: Advanced` hint
- Added `PlayTable Full Match Log` under the Advanced drawer.
- Kept the existing full event/replay panel behavior available through
  `CreateEventReplayPanel()`.
- Added `CreateCompactEventReplayPanel()` for the primary compact panel.

## Preserved Boundaries

- No RulesCore changes.
- No event model changes.
- No replay protocol changes.
- No Photon/network payload changes.
- No card-preview rewrite.
- No private card instance id leak in match log formatting.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m28_10_match_log_density.log`
  - no compiler-error markers
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m28_10_match_log_density.xml`
  - `1151/1151` passed
- Editor client smoke:
  `client/unity/VanguardThaiSim/work/client_smoke_m28_10_match_log_density.log`
  - `blockers=[]`
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m28_10_match_log_density.log`
  - succeeded
  - `errors=0`
  - `warnings=0`
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m28_10_match_log_density.json`
  - `blockers=[]`

## Result

M28 Windows Gameplay Completion Pass is closed for the current Windows-first
scope. The next target should be a Windows playable-loop final audit before
opening any new feature track.
