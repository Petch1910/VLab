# Fourth-Slice Fixture Closeout Spec

Milestone: `M49-closeout`

## Purpose

`M49-closeout` closes the fourth-slice Royal Paladin fixture pipeline after
`M49-05`.

It records whether the repaired recipe enters offline runtime/test fixture
scope and selects the next queue explicitly. It does not enable the fixture in
the live UI, saved deck library, bot runtime, G Zone runtime, or Stride runtime.

## Inputs

- `outputs/target_slice/m49_05_fourth_slice_runtime_fixture_gate.json`

## Outputs

- `outputs/target_slice/m49_closeout_fourth_slice_fixture.json`
- `outputs/target_slice/m49_closeout_fourth_slice_fixture.md`

## Closeout Decision

If the M49-05 gate passed, then:

- `m48_recipe_001` enters offline runtime/test fixture scope
- live runtime deck UI remains disabled
- saved deck injection remains disabled
- bot playbook use remains disabled
- G Zone runtime remains disabled
- Stride runtime remains disabled
- next queue is `M50`

If the M49-05 gate failed, then:

- `m48_recipe_001` remains advisory only
- next queue is `M49-repair`

## Selected Next Queue

`M50`: Fourth Fixture Consumption and Four-Fixture Scale Gate.

Initial bounded tasks:

- `M50-01`: Fourth fixture schema validator
- `M50-02`: Fourth fixture deck text exporter
- `M50-03`: Fourth fixture headless load smoke
- `M50-04`: Four-fixture scale decision

## Boundary

This milestone must not:

- mutate fixture artifacts
- inject saved player decks
- enable UI deck library entries
- enable bot playbook behavior
- enable G Zone runtime
- enable Stride runtime
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_fourth_slice_fixture_closeout.py
python -m unittest tests.test_fourth_slice_fixture_closeout
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M49-closeout` is done when it records the fourth fixture-scope decision, keeps
live runtime/UI/saved-deck/bot/G Zone/Stride use disabled, selects the next
queue explicitly, and the Python tests pass.
