# Ninth-Slice Recipe Pipeline Entry Gate Spec

Milestone: `M67-04`

## Purpose

Decide whether the ninth selected slice may enter offline recipe pipeline work.

This is a gate artifact only. It must not create deck recipes, runtime
fixtures, saved decks, UI deck entries, bot playbooks, G Zone runtime, Stride
runtime, Bloom/token runtime, Lock/Unlock runtime, Legion/Mate runtime, or
mutate `GameState`.

## Inputs

```text
outputs/target_slice/m67_02_ninth_slice_fixture_readiness.json
outputs/target_slice/m67_03_ninth_slice_semantic_compatibility_probe.json
```

Tests may use in-memory M67-02 and M67-03 reports. Real CLI artifacts remain
gated until the real upstream files exist.

## Gate Rules

The gate passes only when:

- `M67-02` fixture/format readiness passed
- `M67-02` marks the selected slice ready for semantic probe
- `M67-03` semantic/compatibility probe passed
- `M67-03` produced at least one candidate edge
- neither input allows runtime or bot promotion

Because the selected slice is `g_series_first`, recipe validation must start
with a fixture scaffold before any recipe validator work. G Zone and Stride
runtime remain disabled.

## Allowed Next Work

If the gate passes, the next queue is `M68`:

```text
M68-01 Ninth-slice fixture scaffold
M68-02 Ninth-slice review packet
M68-03 Ninth-slice recipe draft model
M68-04 Ninth-slice recipe validator
M68-05 Ninth-slice combo-to-recipe consistency
M68-06 Ninth-slice blocker repair candidates
M68-closeout Ninth-slice runtime readiness decision
```

## Runtime Boundary

- Decision artifact only.
- No deck recipe is created.
- No runtime fixture is created.
- No runtime pack mutation.
- No UI or saved-deck publication.
- No bot/playbook publication.
- No G Zone, Stride, Bloom/token, Lock/Unlock, or Legion/Mate runtime
  enablement.
- No `GameState` mutation.

## Outputs

```text
outputs/target_slice/m67_04_ninth_slice_recipe_pipeline_entry_gate.json
outputs/target_slice/m67_04_ninth_slice_recipe_pipeline_entry_gate.md
```

## Verification

```powershell
python -m unittest tests.test_ninth_slice_recipe_pipeline_entry_gate
python -m unittest discover -s tests -p "test_*.py"
```

Real artifact verification after M67-02 and M67-03 outputs exist:

```powershell
python tools\deck\build_ninth_slice_recipe_pipeline_entry_gate.py
```
