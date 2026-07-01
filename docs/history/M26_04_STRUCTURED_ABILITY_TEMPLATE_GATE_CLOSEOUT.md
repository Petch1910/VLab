# M26-04 Structured Ability Template Gate Closeout

## Status

Done.

## What Changed

- Added `StructuredAbilityTemplateAutomationGate`, a pure gate/report that
  allows automated structured ability execution only for template ids with
  named regression coverage.
- Added a JSON-serializable coverage report for current cost, target, effect,
  and duration templates.
- Wired `StructuredAbilityFixtureRunner` through the gate before cost, target,
  effect, or modifier execution.
- Kept unsupported ability paths on manual fallback instead of adding new
  automation.
- Added `STRUCTURED_ABILITY_TEMPLATE_TEST_GATE_SPEC.md`.

## Covered Automated Templates

- Costs: `none`, `counter_blast`, `soul_blast`, `energy_blast`,
  `once_per_turn`, `once_per_fight`
- Targets: `none`, `self`, `unit`, `card` for supported visible zones only
- Effects: `draw`, `move_zone`, `counter_blast`, `counter_charge`,
  `soul_blast`, `soul_charge`, `power_plus`, `critical_plus`
- Durations: `instant`, `until_end_of_turn`, `until_end_of_battle`

## Manual Fallback Boundaries

The gate rejects automation and requires manual fallback for:

- `discard`
- `manual` or unknown effects
- circle targets
- target filters
- hidden/private target zones
- untested modifier durations such as `continuous` or `manual`

## Verification

- Unity compile:
  `client/unity/VanguardThaiSim/work/unity_compile_m26_04_structured_ability_template_gate.log`
  has no compiler-error markers.
- Unity EditMode:
  `client/unity/VanguardThaiSim/work/unity_editmode_m26_04_structured_ability_template_gate.xml`
  passed `1098/1098`.
- Windows build:
  `client/unity/VanguardThaiSim/work/windows_build_m26_04_structured_ability_template_gate.log`
  completed with `errors=0`, `warnings=0`.
- Windows player smoke:
  `client/unity/VanguardThaiSim/work/player_smoke_m26_04_structured_ability_template_gate.json`
  passed with `blockers=[]`.

## Next Target

`M26-05`: keep live effect resolution free of LLM/runtime card-text parsing.
