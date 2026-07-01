# Second-Slice Fixture Closeout Spec

Milestone: `M41-closeout`

## Purpose

`M41-closeout` closes the second-slice Oracle Think Tank fixture pipeline after
`M41-04`.

It records whether the repaired recipe enters offline runtime/test fixture
scope and selects the next queue explicitly. It does not enable the fixture in
the live UI, saved deck library, or bot runtime.

## Inputs

- `outputs/target_slice/m41_04_second_slice_runtime_fixture_promotion_gate.json`

## Outputs

- `outputs/target_slice/m41_closeout_second_slice_fixture.json`
- `outputs/target_slice/m41_closeout_second_slice_fixture.md`

## Closeout Decision

If the M41-04 gate passed, then:

- `m40_recipe_001` enters offline runtime/test fixture scope
- live runtime deck UI remains disabled
- saved deck injection remains disabled
- bot playbook use remains disabled
- next queue is `M42`

If the M41-04 gate failed, then:

- `m40_recipe_001` remains advisory only
- next queue is `M41-repair`

## Selected Next Queue

`M42`: Second Fixture Consumption and Third-Slice Scale Gate.

Initial bounded tasks:

- `M42-01`: Second fixture schema validator
- `M42-02`: Second fixture deck text exporter
- `M42-03`: Second fixture headless load smoke
- `M42-04`: Multi-fixture scale decision

## Boundary

This milestone must not:

- mutate fixture artifacts
- inject saved player decks
- enable UI deck library entries
- enable bot playbook behavior
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_second_slice_fixture_closeout.py
python -m unittest tests.test_second_slice_fixture_closeout
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M41-closeout` is done when it records the second fixture-scope decision, keeps
live runtime/UI/saved-deck/bot use disabled, selects the next queue explicitly,
and the Python tests pass.
