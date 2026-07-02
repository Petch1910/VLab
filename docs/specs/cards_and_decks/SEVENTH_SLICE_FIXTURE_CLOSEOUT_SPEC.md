# Seventh-Slice Fixture Closeout Spec

Milestone: `M61-closeout`

## Purpose

`M61-closeout` closes the seventh-slice Neo Nectar fixture gate and selects the
next queue.

This closeout is a reporting and routing artifact only. It must not mutate the
fixture, inject saved decks, publish UI deck lists, enable bot playbooks,
enable G Zone/Stride runtime, enable Bloom/token runtime, or mutate `GameState`.

## Inputs

- `outputs/target_slice/m61_06_seventh_slice_runtime_fixture_promotion_gate.json`

Tests may pass an in-memory M61-06 gate report until the real upstream artifact
exists.

## Outputs

- `outputs/target_slice/m61_closeout_seventh_slice_fixture.json`
- `outputs/target_slice/m61_closeout_seventh_slice_fixture.md`

## Closeout Rules

- `m61_complete=true` only when M61-06 reports `promotion_allowed=true` and
  `fixture_created=true`.
- If complete, the seventh recipe enters offline runtime/test fixture scope
  only.
- Saved deck, live UI deck list, bot/playbook, G Zone/Stride runtime,
  Bloom/token runtime, and `GameState` use remain disabled.
- If complete, next queue is `M62`.
- If incomplete, next queue is `M61-repair`.

## M62 Queue

If M61 completes, M62 is opened for bounded fixture consumption work:

- `M62-01`: Seventh fixture schema validator
- `M62-02`: Seventh fixture deck text exporter
- `M62-03`: Seventh fixture headless load smoke
- `M62-04`: Seven-fixture scale decision

M62 must continue to treat the fixture as offline evidence only until a later
explicit gate allows saved deck, UI publication, bot/playbook, or live runtime
use.

## Runtime Boundary

This milestone must not:

- modify fixture artifacts
- mutate runtime deck libraries
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- enable G Zone/Stride runtime
- enable Bloom/token effect runtime
- enable token lifecycle runtime
- enable same-name runtime tracking
- mutate `GameState`

## Verification

```powershell
python -m unittest tests.test_seventh_slice_fixture_closeout
python -m unittest discover -s tests -p "test_*.py"
```

The generator can be run only after M61-06 output exists:

```powershell
python tools\deck\build_seventh_slice_fixture_closeout.py
```

## Done Rule

`M61-closeout` is done when:

- complete M61-06 evidence routes to `M62`
- failed M61-06 evidence routes to `M61-repair`
- closeout records Neo Nectar fixture availability without enabling saved
  decks, UI deck publication, bot/playbook, G Zone/Stride runtime, Bloom/token
  runtime, or `GameState` mutation
- docs current target moves to `M62-01`
