# First Runtime Fixture Closeout Spec

Milestone: `M38-closeout`

## Purpose

`M38-closeout` closes the first recipe fixture pipeline after `M38-04`.

It records whether the first accepted recipe enters offline runtime/test
fixture scope and selects the next queue explicitly. It does not enable the
fixture in the live UI, saved deck library, or bot runtime.

## Inputs

- `outputs/target_slice/m38_04_runtime_fixture_promotion_gate.json`

## Outputs

- `outputs/target_slice/m38_closeout_first_runtime_fixture.json`
- `outputs/target_slice/m38_closeout_first_runtime_fixture.md`

## Closeout Decision

If the M38-04 gate passed, then:

- `recipe_003` enters offline runtime/test fixture scope
- live runtime deck UI remains disabled
- bot playbook use remains disabled
- next queue is `M39`

If the M38-04 gate failed, then:

- `recipe_003` remains advisory only
- next queue is `M38-repair`

## Selected Next Queue

`M39`: Fixture Consumption and Second-Slice Scale Gate.

Initial bounded tasks:

- `M39-01`: Offline fixture schema validator
- `M39-02`: Fixture-to-deck text exporter
- `M39-03`: Headless fixture load smoke
- `M39-04`: Second-slice recipe scale decision

## Runtime Boundary

This milestone must not:

- mutate the fixture artifact
- inject saved player decks
- enable UI deck library entries
- enable bot playbook behavior
- mutate `GameState`

## Verification

```powershell
python tools\deck\build_first_runtime_fixture_closeout.py
python -m unittest tests.test_first_runtime_fixture_closeout
python -m unittest discover -s tests -p "test_*.py"
```

## Done Rule

`M38-closeout` is done when it records the first fixture-scope decision, keeps
live runtime and bot use disabled, selects the next queue explicitly, and the
Python tests pass.
