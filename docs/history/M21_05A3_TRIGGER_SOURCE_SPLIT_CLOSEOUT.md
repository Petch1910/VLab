# M21-05a3 Trigger Source Split Closeout

Status: Done

## Scope

- Added `TriggerCheckSource` metadata to `LegalGameAction` and `GameEvent`.
- Updated legal battle actions to expose separate Drive and Damage trigger-check
  actions.
- Updated RulesCore trigger-check matching to preserve backward compatibility
  for legacy/manual requests while matching explicit Drive/Damage source when
  present.
- Updated PlayTable to show separate `Drive` and `Damage Check` buttons.
- Updated player-facing event/replay log formatting to show trigger-check source
  without leaking checked private card instance ids.

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m21_05c_trigger_source_split.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m21_05c_trigger_source_split.xml`
  passed `941/941`.
- Windows local player build for smoke:
  `client/unity/VanguardThaiSim/work/windows_build_m21_05c_trigger_source_split.log`
  reports `Succeeded`, `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m21_05c_trigger_source_split.json`
  passed with `blockers=[]`.

## Next

Continue M21-05 with attack declaration selection flow or manual note polish.
