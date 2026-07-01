# Fifth-Slice Fixture Closeout Spec

Milestone: `M53-closeout`

## Purpose

`M53-closeout` closes the fifth-slice Gold Paladin fixture gate and selects the
next queue.

This closeout is a reporting and routing artifact only. It must not mutate the
fixture, inject saved decks, publish UI deck lists, enable bot playbooks, or
mutate `GameState`.

## Inputs

- `outputs/target_slice/m53_05_fifth_slice_runtime_fixture_promotion_gate.json`

## Outputs

- `outputs/target_slice/m53_closeout_fifth_slice_fixture.json`
- `outputs/target_slice/m53_closeout_fifth_slice_fixture.md`

## Closeout Rules

- `m53_complete=true` only when M53-05 reports `promotion_allowed=true` and
  `fixture_created=true`.
- If complete, the fifth recipe enters offline runtime/test fixture scope only.
- Saved deck, live UI deck list, bot/playbook, and `GameState` use remain
  disabled.
- If complete, next queue is `M54`.
- If incomplete, next queue is `M53-repair`.

## Runtime Boundary

This milestone must not:

- modify fixture artifacts
- mutate runtime deck libraries
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- mutate `GameState`

## Verification

```powershell
python -m unittest tests.test_fifth_slice_fixture_closeout
python -m unittest discover -s tests -p "test_*.py"
```

The generator can be run only after M53-05 output exists:

```powershell
python tools\deck\build_fifth_slice_fixture_closeout.py
```
