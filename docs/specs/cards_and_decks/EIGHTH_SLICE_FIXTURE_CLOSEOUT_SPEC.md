# Eighth-Slice Fixture Closeout Spec

Milestone: `M65-closeout`

## Purpose

`M65-closeout` closes the eighth-slice Kagero runtime fixture gate and selects
the next bounded queue.

This closeout is a reporting and routing artifact only. It must not mutate the
fixture, inject saved decks, publish UI deck lists, enable bot playbooks,
enable Lock/Unlock runtime, enable Legion/Mate runtime, or mutate `GameState`.

## Inputs

- `outputs/target_slice/m65_06_eighth_slice_runtime_fixture_promotion_gate.json`

Tests may pass an in-memory M65-06 gate report until the real upstream artifact
exists.

## Outputs

- `outputs/target_slice/m65_closeout_eighth_slice_fixture.json`
- `outputs/target_slice/m65_closeout_eighth_slice_fixture.md`

## Closeout Rules

- `m65_complete=true` only when M65-06 reports `promotion_allowed=true`,
  `fixture_created=true`, and includes an `offline_runtime_test_fixture`
  runtime fixture payload.
- If complete, the eighth recipe enters offline runtime/test fixture scope
  only.
- Saved deck, live UI deck list, bot/playbook, Lock/Unlock runtime,
  Legion/Mate runtime, and `GameState` use remain disabled.
- If complete, next queue is `M66`.
- If incomplete, next queue is `M65-repair`.

## M66 Queue

If M65 completes, M66 is opened for bounded fixture consumption work:

- `M66-01`: Eighth fixture schema validator
- `M66-02`: Eighth fixture deck text exporter
- `M66-03`: Eighth fixture headless load smoke
- `M66-04`: Eight-fixture scale decision

M66 must continue to treat the fixture as offline evidence only until a later
explicit gate allows saved deck, UI publication, bot/playbook, or live runtime
use.

## Runtime Boundary

This milestone must not:

- modify fixture artifacts
- mutate runtime deck libraries
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- enable Lock/Unlock runtime
- enable Legion/Mate runtime
- enable Mate identity checks
- mutate `GameState`

## Verification

```powershell
python -m unittest tests.test_eighth_slice_fixture_closeout
python -m unittest discover -s tests -p "test_*.py"
```

The generator can be run only after M65-06 output exists:

```powershell
python tools\deck\build_eighth_slice_fixture_closeout.py
```

## Done Rule

`M65-closeout` is done when:

- complete M65-06 evidence routes to `M66`
- failed or payload-missing M65-06 evidence routes to `M65-repair`
- closeout records Kagero fixture availability without enabling saved decks,
  UI deck publication, bot/playbook, Lock/Unlock runtime, Legion/Mate runtime,
  or `GameState` mutation
- docs current target moves to `M66-01`
