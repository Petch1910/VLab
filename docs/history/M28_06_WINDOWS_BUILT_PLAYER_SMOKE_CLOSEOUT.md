# M28-06 Windows Built-Player Smoke Closeout

## Scope

`M28-06` rebuilds the Windows player after the M28 PlayTable UI changes and
runs the built-player smoke path from the actual executable.

This is local Windows verification only. It is not release packaging, public
distribution, Android work, APK work, or mobile QA.

## Verification

- Windows build log:
  `client/unity/VanguardThaiSim/work/windows_build_m28_06_built_player_smoke.log`
- Build result:
  `Succeeded`
- Build output:
  `client/unity/VanguardThaiSim/build/windows/latest/VanguardThaiSim.exe`
- Build summary:
  `errors=0`, `warnings=0`
- Runtime pack copied to:
  `client/unity/VanguardThaiSim/build/windows/latest/data/packs/vanguard_th`
- Built-player smoke report:
  `client/unity/VanguardThaiSim/work/player_smoke_m28_06_built_player_smoke.json`
- Built-player smoke result:
  `blockers=[]`

Player smoke covered:

- Card Browser data load and query smoke.
- Deck Builder playable deck and deck-code round-trip.
- Home status and Solo setup readiness.
- Manual content/tips/originality readiness.
- Settings default/cycle smoke.
- Photon trusted-client room usability guard.
- PlayTable Windows gameplay completion smoke with 16 committed events.
- Windows board-first layout reference viewports.

## Result

`M28-06` is complete. The current Windows executable launches the smoke path and
passes the same local workflow gates as the editor smoke.

## Next Target

`M28-07` should polish PlayTable action grouping and labels without changing
RulesCore. The goal is to make the dense action rows easier to scan after the
M28-05 next-action guidance panel.
