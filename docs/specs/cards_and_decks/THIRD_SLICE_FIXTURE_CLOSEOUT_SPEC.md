# Third-Slice Fixture Closeout Spec

Milestone: `M45-closeout`

## Purpose

`M45-closeout` closes the third-slice Bermuda Triangle fixture pipeline after
`M45-04`.

It records whether the repaired recipe enters offline runtime/test fixture
scope and selects the next queue explicitly. It does not enable the fixture in
the live UI, saved deck library, or bot runtime.

## Inputs

- `outputs/target_slice/m45_04_third_slice_runtime_fixture_promotion_gate.json`

## Outputs

- `outputs/target_slice/m45_closeout_third_slice_fixture.json`
- `outputs/target_slice/m45_closeout_third_slice_fixture.md`

## Closeout Decision

If the M45-04 gate passed, then:

- `m44_recipe_001` enters offline runtime/test fixture scope
- live runtime deck UI remains disabled
- saved deck injection remains disabled
- bot playbook use remains disabled
- next queue is `M46`

If the M45-04 gate failed, then:

- `m44_recipe_001` remains advisory only
- next queue is `M45-repair`

## Selected Next Queue

`M46`: Third Fixture Consumption and Multi-Fixture Scale Gate.

Initial bounded tasks:

- `M46-01`: Third fixture schema validator
- `M46-02`: Third fixture deck text exporter
- `M46-03`: Third fixture headless load smoke
- `M46-04`: Multi-fixture scale decision

## Boundary

This milestone must not:

- mutate fixture artifacts
- inject saved player decks
- enable UI deck library entries
- enable bot playbook behavior
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_third_slice_fixture_closeout.py
python -m unittest tests.test_third_slice_fixture_closeout
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M45-closeout` is done when it records the third fixture-scope decision, keeps
live runtime/UI/saved-deck/bot use disabled, selects the next queue explicitly,
and the Python tests pass.
