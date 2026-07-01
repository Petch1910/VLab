# M21-08 Tests And Windows Smoke Closeout

Status: Done

## Scope

- Rolled up the PlayTable Windows UX verification evidence from `M21-02`
  through `M21-07`.
- Current formatter coverage includes:
  - setup readiness/status guidance
  - selected-card preview
  - zone status
  - battle flow status
  - manual notes
  - Advanced drawer placement
  - player-readable event/replay log
- Current UI/runtime coverage includes:
  - PlayTable runtime UI creation
  - hidden-by-default Advanced drawer
  - diagnostic controls parented under Advanced UI
  - no direct state mutation for local notes and network/debug payload storage
  - hidden-state safe display view creation
- Current Windows smoke still covers Card Browser, Deck Builder, PlayTable
  RulesCore actions, and board-first layout reference viewports.

## Verification Evidence

- Latest Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_07_player_event_log.log`
  has no compiler-error markers.
- Latest Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_07_player_event_log.xml`
  passed `982/982`.
- Latest Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_07_player_event_log.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Latest Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_07_player_event_log.json`
  passed with `blockers=[]`.

## Notes

- No new runtime code was required for this roll-up task after `M21-07`.
- Android, APK, mobile QA, app packaging, release-candidate, and public
  distribution work remain deferred.

## Next

Move to `M21-09`: close out the PlayTable Windows UX pass and set `M22`
Windows Settings / Deck Type / Accessories as the next product target.
