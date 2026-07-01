# Sixth-Slice Fixture Closeout Spec

Milestone: `M57-closeout`

## Purpose

`M57-closeout` closes the sixth-slice Shadow Paladin fixture gate and selects
the next queue.

This closeout is a reporting and routing artifact only. It must not mutate the
fixture, inject saved decks, publish UI deck lists, enable bot playbooks,
enable G Zone/Stride runtime, or mutate `GameState`.

## Inputs

- `outputs/target_slice/m57_06_sixth_slice_runtime_fixture_promotion_gate.json`

## Outputs

- `outputs/target_slice/m57_closeout_sixth_slice_fixture.json`
- `outputs/target_slice/m57_closeout_sixth_slice_fixture.md`

## Closeout Rules

- `m57_complete=true` only when M57-06 reports `promotion_allowed=true` and
  `fixture_created=true`.
- If complete, the sixth recipe enters offline runtime/test fixture scope only.
- Saved deck, live UI deck list, bot/playbook, G Zone/Stride runtime, and
  `GameState` use remain disabled.
- If complete, next queue is `M58`.
- If incomplete, next queue is `M57-repair`.

## Runtime Boundary

This milestone must not:

- modify fixture artifacts
- mutate runtime deck libraries
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- enable G Zone/Stride runtime
- mutate `GameState`

## Verification

```powershell
python -m unittest tests.test_sixth_slice_fixture_closeout
python -m unittest discover -s tests -p "test_*.py"
```

The generator can be run only after M57-06 output exists:

```powershell
python tools\deck\build_sixth_slice_fixture_closeout.py
```
