# M26 Bot / Automation Return Gate Closeout

## Status

Complete.

## What Closed

M26 reopened bot/automation work after the Windows UX and multiplayer passes,
but kept it behind correctness-first gates. The completed slices are:

- `M26-01`: prerequisite audit
- `M26-02`: legal-action and masked-state bot gate
- `M26-03`: player-readable bot explanation panel
- `M26-04`: structured ability template test gate
- `M26-05`: no live card-text or LLM effect parsing
- `M26-06`: Home Solo Practice setup flow
- `M26-07`: safety regression gate for hidden state, snapshot simulation, and
  replay determinism

## Current Safe Boundary

CPU/bot work may continue only through:

- `RulesCore.GetLegalActions`
- masked player/bot views
- snapshot simulation paths that do not mutate live state
- structured ability templates with tests
- replay/event logs for traceability

Do not add advanced ISMCTS, RL/self-play, runtime card-text parsing, or hidden
state access without a new explicit milestone.

## Verification

- M26-06 Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m26_06_solo_play_entry_flow.log`
- M26-06 Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m26_06_solo_play_entry_flow.json`
  passed with `blockers=[]`
- M26-07 Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m26_07_bot_automation_safety_regression.log`
- M26-07 Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m26_07_bot_automation_safety_regression.xml`
  passed `1119/1119`

## Next Target

`M27-01`: Windows stability smoke coverage for Home, Deck Builder, PlayTable,
Manual, Settings, and Online Room.
