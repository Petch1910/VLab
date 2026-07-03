# Ninth-Slice Fixture Closeout Spec

Milestone: `M69-closeout`

## Purpose

`M69-closeout` closes the ninth-slice Aqua Force runtime fixture gate and
selects the next bounded queue.

This closeout is a reporting and routing artifact only. It must not mutate the
fixture, inject saved decks, publish UI deck lists, enable bot playbooks,
enable G Zone runtime, enable Stride runtime, enable Aqua Force battle-order
runtime, parse live card text, or mutate `GameState`.

## Inputs

- `outputs/target_slice/m69_06_ninth_slice_runtime_fixture_promotion_gate.json`

Tests may pass an in-memory M69-06 gate report until the real upstream artifact
exists.

## Outputs

- `outputs/target_slice/m69_closeout_ninth_slice_fixture.json`
- `outputs/target_slice/m69_closeout_ninth_slice_fixture.md`

## Closeout Rules

- `m69_complete=true` only when M69-06 reports `promotion_allowed=true`,
  `fixture_created=true`, and includes an `offline_runtime_test_fixture`
  runtime fixture payload.
- If complete, the ninth recipe enters offline runtime/test fixture scope only.
- Saved deck, live UI deck list, bot/playbook, live card text parsing, G Zone
  runtime, Stride runtime, Aqua Force battle-order runtime, and `GameState` use
  remain disabled.
- If complete, next queue is `M70`.
- If incomplete, next queue is `M69-repair`.

## M70 Queue

If M69 completes, M70 is opened for bounded fixture consumption work:

- `M70-01`: Ninth fixture schema validator
- `M70-02`: Ninth fixture deck text exporter
- `M70-03`: Ninth fixture headless load smoke
- `M70-04`: Nine-fixture scale decision

M70 must continue to treat the fixture as offline evidence only until a later
explicit gate allows saved deck, UI publication, bot/playbook, or live runtime
use.

## Runtime Boundary

This milestone must not:

- modify fixture artifacts
- mutate runtime deck libraries
- inject saved decks
- publish UI deck lists
- enable bot/playbook integration
- parse live card text at runtime
- enable G Zone runtime
- enable Stride runtime
- enable Aqua Force battle-order runtime
- mutate `GameState`

## Verification

```powershell
python -m unittest tests.test_ninth_slice_fixture_closeout
python -m unittest discover -s tests -p "test_*.py"
```

The generator can be run only after M69-06 output exists:

```powershell
python tools\deck\build_ninth_slice_fixture_closeout.py
```

## Done Rule

`M69-closeout` is done when:

- complete M69-06 evidence routes to `M70`
- failed or payload-missing M69-06 evidence routes to `M69-repair`
- closeout records Aqua Force fixture availability without enabling saved
  decks, UI deck publication, bot/playbook, live text parsing, G Zone runtime,
  Stride runtime, Aqua Force battle-order runtime, or `GameState` mutation
- docs current target moves to `M70-01`
